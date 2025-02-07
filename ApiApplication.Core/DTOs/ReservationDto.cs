using ApiApplication.Domain.Entities;

namespace ApiApplication.Core.DTOs
{
    public class ReserveSeatsDto
    {
        public int ShowtimeId { get; set; }
        public int AuditoriumId { get; set; }
        public List<SeatDto> Seats { get; set; } = [];
    }

    public class ReservationResponseDto
    {
        public Guid ReservationId { get; set; }
        public int AuditoriumId { get; set; }
        public int NumberOfSeats { get; set; }
        public int ShowtimeId { get; set; }
        public string? MovieTitle { get; set; }
        public List<SeatDto> SeatsReserved { get; set; } = [];
        public DateTime ReservationExpirationTime { get; set; }
    }
}