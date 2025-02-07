using ApiApplication.Core.DTOs;
using ApiApplication.Core.Services;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Domain.Entities;
using ApiApplication.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiApplication.Tests.ServiceTests
{

    [TestFixture]
    public class ReservationServiceTests
    {
        private Mock<ITicketsRepository> _mockTicketsRepository;
        private Mock<IShowtimesRepository> _mockShowtimesRepository;
        private Mock<IAuditoriumsRepository> _mockAuditoriumsRepository;
        private Mock<ILogger<ReservationService>> _mockLogger;
        private ReservationService _service;

        [SetUp]
        public void SetUp()
        {
            _mockTicketsRepository = new Mock<ITicketsRepository>();
            _mockShowtimesRepository = new Mock<IShowtimesRepository>();
            _mockAuditoriumsRepository = new Mock<IAuditoriumsRepository>();
            _mockLogger = new Mock<ILogger<ReservationService>>();

            _service = new ReservationService(
                _mockTicketsRepository.Object,
                _mockShowtimesRepository.Object,
                _mockAuditoriumsRepository.Object,
                _mockLogger.Object);
        }

        [Test]
        public void ReserveSeatsAsync_ShouldThrowException_WhenShowtimeNotFound()
        {
            var request = new ReserveSeatsDto { ShowtimeId = 1, AuditoriumId = 1, Seats = [] };
            _mockShowtimesRepository.Setup(r => r.GetWithMoviesByIdAsync(request.ShowtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShowtimeEntity)null!);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ReserveSeatsAsync(request, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Showtime not found."));
        }

        [Test]
        public void ReserveSeatsAsync_ShouldThrowException_WhenAuditoriumNotFound()
        {
            var request = new ReserveSeatsDto { ShowtimeId = 1, AuditoriumId = 1, Seats = new List<SeatDto>() };
            _mockShowtimesRepository.Setup(r => r.GetWithMoviesByIdAsync(request.ShowtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ShowtimeEntity());
            _mockAuditoriumsRepository.Setup(r => r.GetAsync(request.AuditoriumId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AuditoriumEntity)null!);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ReserveSeatsAsync(request, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Auditorium not found."));
        }

        [Test]
        public void ReserveSeatsAsync_ShouldThrowException_WhenAuditoriumHasNoShowtime()
        {
            var request = new ReserveSeatsDto { ShowtimeId = 1, AuditoriumId = 4, Seats = new List<SeatDto>() };
            _mockShowtimesRepository.Setup(r => r.GetWithMoviesByIdAsync(request.ShowtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ShowtimeEntity());
            _mockAuditoriumsRepository.Setup(r => r.GetAsync(request.AuditoriumId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new AuditoriumEntity { });
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ReserveSeatsAsync(request, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Auditorium does not have this showtime."));
        }


        [Test]
        public void ReserveSeatsAsync_ShouldThrowException_WhenSeatsAreNotAvailable()
        {
            // Arrange
            var reserveSeatsDto = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = new List<SeatDto>
                {
                    new() { RowNumber = 1, SeatNumber = 1 },
                    new() { RowNumber = 1, SeatNumber = 2 }
                }
            };

            var bookedTickets = new List<TicketEntity>
            {
                new()
                {
                    Paid = true,
                    CreatedTime = DateTime.Now,
                    Seats = new List<SeatEntity>
                    {
                        new() { Row = 1, SeatNumber = 1 } // Seat 1 is already booked
                    }
                }
            };

            _mockShowtimesRepository.Setup(repo => repo.GetWithMoviesByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ShowtimeEntity { AuditoriumId = 1 });

            _mockAuditoriumsRepository.Setup(repo => repo.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuditoriumEntity { Id = 1 });

            _mockTicketsRepository.Setup(repo => repo.GetEnrichedAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookedTickets);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.ReserveSeatsAsync(reserveSeatsDto, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("One or more seats are not available."));
        }

        [Test]
        public void ReserveSeatsAsync_ShouldThrowException_WhenSeatsAreNotContiguous()
        {
            // Arrange
            var reserveSeatsDto = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats =
                [
                    new() { RowNumber = 1, SeatNumber = 1 },
                    new() { RowNumber = 1, SeatNumber = 3 } // Seat 2 is missing (non-contiguous)
                ]
            };

            _mockShowtimesRepository.Setup(repo => repo.GetWithMoviesByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ShowtimeEntity { AuditoriumId = 1 });

            _mockAuditoriumsRepository.Setup(repo => repo.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuditoriumEntity { Id = 1 });

            _mockTicketsRepository.Setup(repo => repo.GetEnrichedAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]); // No pre-booked seats

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.ReserveSeatsAsync(reserveSeatsDto, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Seats must be contiguous."));
        }

        [Test]
        public async Task ReserveSeatsAsync_ShouldReturnReservationResponse_WhenSeatsAreAvailableAndContiguous()
        {
            // Arrange
            var reserveSeatsDto = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats =
                [
                    new() { RowNumber = 1, SeatNumber = 1 },
                    new() { RowNumber = 1, SeatNumber = 2 }
                ]
            };

            var showtime = new ShowtimeEntity { Id = 1, Movie = new MovieEntity { Title = "Test Movie" }, AuditoriumId = 1 };
            var auditorium = new AuditoriumEntity { Id = 1 };
            var generatedTicket = new TicketEntity { Id = Guid.NewGuid(), CreatedTime = DateTime.Now };

            _mockShowtimesRepository.Setup(repo => repo.GetWithMoviesByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtime);

            _mockAuditoriumsRepository.Setup(repo => repo.GetAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auditorium);

            _mockTicketsRepository.Setup(repo => repo.GetEnrichedAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TicketEntity>());

            _mockTicketsRepository.Setup(repo => repo.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<SeatEntity>>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(generatedTicket);

            // Act
            var result = await _service.ReserveSeatsAsync(reserveSeatsDto, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReservationId, Is.EqualTo(generatedTicket.Id));
            Assert.That(result.MovieTitle, Is.EqualTo("Test Movie"));
            Assert.That(result.SeatsReserved.Count, Is.EqualTo(2));
        }
    }
}