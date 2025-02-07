using ApiApplication.Controllers;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ApiApplication.Tests.ControllerTests
{
    [TestFixture]
    public class ReservationControllerTests
    {
        private Mock<IReservationService> _mockReservationService;
        private ReservationController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockReservationService = new Mock<IReservationService>();
            _controller = new ReservationController(_mockReservationService.Object);
        }

        [Test]
        public async Task ReserveSeats_ShouldReturnSuccess_WhenReservationSucceeds()
        {
            // Arrange
            var request = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }]
            };

            var reservationId = Guid.NewGuid();

            var expectedResponse = new ReservationResponseDto
            {
                ReservationId = reservationId,
                ShowtimeId = 1,
                AuditoriumId = 1,
                SeatsReserved = request.Seats,
                ReservationExpirationTime = DateTime.Now.AddMinutes(10)
            };
            _mockReservationService.Setup(s => s.ReserveSeatsAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.ReserveSeats(request) as OkObjectResult;
            var response = result?.Value as CommonResult<ReservationResponseDto>;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(response, Is.Not.Null);
            });

            Assert.That(response.Data.ReservationId, Is.EqualTo(expectedResponse.ReservationId));
        }
    }
}
