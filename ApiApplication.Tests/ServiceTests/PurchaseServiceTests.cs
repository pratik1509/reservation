using ApiApplication.Core.DTOs;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiApplication.Tests.ServiceTests
{
    [TestFixture]
    public class PurchaseServiceTests
    {
        private Mock<ITicketsRepository> _ticketsRepositoryMock;
        private Mock<ILogger<PurchaseService>> _loggerMock;
        private PurchaseService _service;

        [SetUp]
        public void Setup()
        {
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _loggerMock = new Mock<ILogger<PurchaseService>>();
            _service = new PurchaseService(_ticketsRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task BuySeatsAsync_ReturnsSuccess_WhenReservationIsValid()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var ticket = new TicketEntity { Id = reservationId, CreatedTime = DateTime.Now, Paid = false };

            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _ticketsRepositoryMock
                .Setup(x => x.ConfirmPaymentAsync(ticket, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            var result = await _service.BuySeatsAsync(new BuySeatsDto { ReservationId = reservationId }, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Reservation confirmed successfully."));

            _ticketsRepositoryMock.Verify(x => x.ConfirmPaymentAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task BuySeatsAsync_ReturnsError_WhenReservationNotFound()
        {
            // Arrange
            var reservationId = Guid.NewGuid();

            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TicketEntity)null!);

            // Act
            var result = await _service.BuySeatsAsync(new BuySeatsDto { ReservationId = reservationId }, CancellationToken.None);
            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Reservation not found."));
        }

        [Test]
        public async Task BuySeatsAsync_ReturnsError_WhenReservationIsExpired()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var ticket = new TicketEntity { Id = reservationId, CreatedTime = DateTime.Now.AddMinutes(-10), Paid = false };

            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            var result = await _service.BuySeatsAsync(new BuySeatsDto { ReservationId = reservationId }, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Reservation has expired."));

        }

        [Test]
        public async Task BuySeatsAsync_ReturnsError_WhenTicketsAreAlreadyPaid()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var ticket = new TicketEntity { Id = reservationId, CreatedTime = DateTime.Now, Paid = true };

            _ticketsRepositoryMock
                .Setup(x => x.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            var result = await _service.BuySeatsAsync(new BuySeatsDto { ReservationId = reservationId }, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Tickets already paid."));
        }
    }
}
