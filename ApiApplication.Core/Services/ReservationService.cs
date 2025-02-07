using System.Diagnostics;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Domain.Entities;
using ApiApplication.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApiApplication.Core.Services
{
    public class ReservationService(
        ITicketsRepository ticketsRepository,
        IShowtimesRepository showtimesRepository,
        IAuditoriumsRepository auditoriumsRepository,
        ILogger<ReservationService> logger) : IReservationService
    {
        public async Task<ReservationResponseDto> ReserveSeatsAsync(ReserveSeatsDto reserveSeatsDto, CancellationToken cancel)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var showtime = await showtimesRepository.GetWithMoviesByIdAsync(reserveSeatsDto.ShowtimeId, cancel);
                if (showtime == null)
                {
                    throw new Exception("Showtime not found.");
                }

                var auditorium = await auditoriumsRepository.GetAsync(reserveSeatsDto.AuditoriumId, cancel);
                if (auditorium == null)
                {
                    throw new Exception("Auditorium not found.");
                }

                if (showtime.AuditoriumId != reserveSeatsDto.AuditoriumId)
                {
                    throw new Exception("Auditorium does not have this showtime.");
                }

                var tickets = await ticketsRepository.GetEnrichedAsync(reserveSeatsDto.ShowtimeId, cancel);

                if (tickets?.Any() == true && !AreRequestedSeatsAvailable(tickets, reserveSeatsDto.Seats))
                {
                    throw new Exception("One or more seats are not available.");
                }

                if (!AreSeatsContiguous(reserveSeatsDto.Seats))
                {
                    throw new Exception("Seats must be contiguous.");
                }

                var generatedTicket = await ticketsRepository.CreateAsync(
                    showtime,
                    [..reserveSeatsDto.Seats.Select(x => new SeatEntity {
                        Row = x.RowNumber,
                        SeatNumber = x.SeatNumber,
                    })],
                    cancel
                );

                var expirationTime = DateTime.Now.AddMinutes(10);

                var reservationResponseDto = new ReservationResponseDto
                {
                    ReservationId = generatedTicket.Id,
                    AuditoriumId = showtime.AuditoriumId,
                    NumberOfSeats = reserveSeatsDto.Seats.Count,
                    ShowtimeId = showtime.Id,
                    MovieTitle = showtime.Movie.Title,
                    SeatsReserved = reserveSeatsDto.Seats,
                    ReservationExpirationTime = generatedTicket.CreatedTime.AddMinutes(10),
                };

                return reservationResponseDto;
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"ReserveSeatsAsync executed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private static bool AreRequestedSeatsAvailable(IEnumerable<TicketEntity> tickets, List<SeatDto> requestedSeats)
        {
            var bookedOrReservedTickets = tickets
                .Where(x => x.Paid || (x.Paid == false && DateTime.Now <= x.CreatedTime.AddMinutes(10)))
                .ToList();

            var bookedOrReservedSeats = bookedOrReservedTickets
                .SelectMany(x => x.Seats)
                .Select(seat => new SeatDto
                {
                    RowNumber = seat.Row,
                    SeatNumber = seat.SeatNumber,
                })
                .ToList();

            if (bookedOrReservedSeats.Count > 0)
            {
                return !requestedSeats.Any(seat => bookedOrReservedSeats.Any(b =>
                    b.RowNumber == seat.RowNumber && b.SeatNumber == seat.SeatNumber));
            }

            return true; // no reservation
        }


        /// <summary>
        /// Find contiguous seats. PS -Here not finding contiguous seats across row number because every auditorium can have diff no of seats
        /// </summary>
        /// <param name="seats"></param>
        /// <returns></returns>
        private static bool AreSeatsContiguous(List<SeatDto> seats)
        {
            var sortedSeats = seats
                .OrderBy(seat => seat.RowNumber)
                .ThenBy(seat => seat.SeatNumber)
                .ToList();

            for (int i = 1; i < sortedSeats.Count; i++)
            {
                if (sortedSeats[i].RowNumber != sortedSeats[i - 1].RowNumber ||
                    sortedSeats[i].SeatNumber != sortedSeats[i - 1].SeatNumber + 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
