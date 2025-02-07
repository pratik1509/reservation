using Moq;
using ApiApplication.Core.Services;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Infrastructure.Abstractions;
using ApiApplication.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using ApiApplication.Domain.Entities;

namespace ApiApplication.Tests.ServiceTests
{
    [TestFixture]
    public class ShowtimeServiceTests
    {
        private Mock<IShowtimesRepository> _mockShowtimesRepository;
        private Mock<IApiClientGrpc> _mockApiClientGrpc;
        private Mock<IRedisCacheService> _mockRedisCacheService;
        private Mock<ILogger<ShowtimeService>> _mockLogger;
        private ShowtimeService _service;

        [SetUp]
        public void Setup()
        {
            _mockShowtimesRepository = new Mock<IShowtimesRepository>();
            _mockApiClientGrpc = new Mock<IApiClientGrpc>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();
            _mockLogger = new Mock<ILogger<ShowtimeService>>();
            _service = new ShowtimeService(
                _mockShowtimesRepository.Object,
                _mockApiClientGrpc.Object,
                _mockRedisCacheService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task CreateShowtimeAsync_ShouldReturnShowtimeResponseDto_WhenValidDataIsProvided()
        {
            // Arrange
            var createShowtimeDto = new CreateShowtimeDto
            {

                ImdbId = "12345",
                SessionDate = new DateTime(2025, 2, 5),
                AuditoriumId = 1
            };

            var showResponseDto = new ShowResponseDto
            {
                Id = "12345",
                Title = "Test Movie",
                Crew = "Test Crew",
                Year = "2025"
            };

            _mockRedisCacheService.Setup(service => service.GetCacheAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            _mockApiClientGrpc.Setup(client => client.GetById(It.IsAny<string>()))
                .ReturnsAsync(new ProtoDefinitions.showResponse
                {
                    Id = "12345",
                    Title = "Test Movie",
                    Crew = "Test Crew",
                    Year = "2025"
                });

            var showtimeEntity = new ShowtimeEntity
            {
                Id = 1,
                Movie = new MovieEntity { Title = "Test Movie" },
                SessionDate = new DateTime(2025, 2, 5),
                AuditoriumId = 1
            };

            _mockShowtimesRepository.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtimeEntity);

            var result = await _service.CreateShowtimeAsync(createShowtimeDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MovieTitle, Is.EqualTo("Test Movie"));
            Assert.That(result.ShowtimeId, Is.EqualTo(1));
            Assert.That(result.SessionDate.ToString("yyyy-MM-dd"), Is.EqualTo("2025-02-05"));
        }

        [Test]
        public void CreateShowtimeAsync_ShouldThrowException_WhenShowNotFound()
        {
            // Arrange
            var createShowtimeDto = new CreateShowtimeDto
            {
                ImdbId = "12345",
                SessionDate = new DateTime(2025, 2, 5), // Using DateTime object
                AuditoriumId = 1
            };

            _mockRedisCacheService.Setup(service => service.GetCacheAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            _mockApiClientGrpc.Setup(client => client.GetById(It.IsAny<string>()))
                .ReturnsAsync((ProtoDefinitions.showResponse?)null!);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.CreateShowtimeAsync(createShowtimeDto));
            Assert.That(ex.Message, Is.EqualTo("Show not found"));
        }

    }
}
