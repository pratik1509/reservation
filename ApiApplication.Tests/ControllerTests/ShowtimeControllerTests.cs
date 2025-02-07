using Moq;
using ApiApplication.Controllers;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Tests.ControllerTests
{
    [TestFixture]
    public class ShowtimeControllerTests
    {
        private Mock<IShowtimeService> _mockShowtimeService;
        private ShowtimeController _controller;

        [SetUp]
        public void Setup()
        {
            _mockShowtimeService = new Mock<IShowtimeService>();
            _controller = new ShowtimeController(_mockShowtimeService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task CreateShowtime_ShouldReturnSuccessResult()
        {
            // Arrange
            var createShowtimeDto = new CreateShowtimeDto
            {
                ImdbId = "12345",
                SessionDate = new DateTime(2025, 02, 05),
                AuditoriumId = 1
            };

            var showtimeResponseDto = new ShowtimeResponseDto
            {
                ShowtimeId = 1,
                MovieTitle = "Test Movie",
                AuditoriumId = 1,
                SessionDate = new DateTime(2025, 02, 05),
            };

            _mockShowtimeService.Setup(service => service.CreateShowtimeAsync(It.IsAny<CreateShowtimeDto>()))
                                .ReturnsAsync(showtimeResponseDto);

            // Act
            var result = await _controller.CreateShowtime(createShowtimeDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Expected OkObjectResult, but got null.");
            Assert.That(okResult.StatusCode, Is.EqualTo(200), "Expected status code 200, but got a different value.");

            var commonResult = okResult.Value as CommonResult<ShowtimeResponseDto>;
            Assert.That(commonResult, Is.Not.Null, "Expected CommonResult<ShowtimeResponseDto>, but got null.");
            Assert.That(commonResult.Success, Is.True, "Expected success to be true, but it was false.");
            Assert.That(commonResult.Data.MovieTitle, Is.EqualTo("Test Movie"), "Movie title doesn't match the expected value.");
        }
    }
}
