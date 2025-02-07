using ApiApplication.Controllers;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ApiApplication.Tests.ControllerTests
{
    [TestFixture]
    public class PurchaseControllerTests
    {
        private Mock<IPurchaseService> _purchaseServiceMock;
        private PurchaseController _controller;

        [SetUp]
        public void Setup()
        {
            _purchaseServiceMock = new Mock<IPurchaseService>();
            _controller = new PurchaseController(_purchaseServiceMock.Object);
        }

        [Test]
        public async Task BuySeats_ReturnsSuccess_WhenPurchaseIsSuccessful()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var response = new PurchaseResponseDto { Success = true, Message = "Reservation confirmed successfully." };

            _purchaseServiceMock
                .Setup(x => x.BuySeatsAsync(It.IsAny<BuySeatsDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.BuySeats(reservationId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var actualResponse = result.Value as CommonResult<PurchaseResponseDto>;
            Assert.That(actualResponse, Is.Not.Null);
            Assert.That(actualResponse!.Data.Success, Is.True);
            Assert.That(actualResponse.Data.Message, Is.EqualTo("Reservation confirmed successfully."));
        }

        [Test]
        public async Task BuySeats_ReturnsError_WhenPurchaseFails()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var response = new PurchaseResponseDto { Success = false, Message = "Reservation has expired." };

            _purchaseServiceMock
                .Setup(x => x.BuySeatsAsync(It.IsAny<BuySeatsDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.BuySeats(reservationId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var actualResponse = result.Value as CommonResult<PurchaseResponseDto>;
            Assert.That(actualResponse, Is.Not.Null);
            Assert.That(actualResponse!.Data.Success, Is.False);
            Assert.That(actualResponse.Data.Message, Is.EqualTo("Reservation has expired."));

        }
    }
}
