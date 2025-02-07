using ApiApplication.Core.DTOs;
using ApiApplication.Database;
using ApiApplication.Infrastructure.Abstractions;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace ApiApplication.IntegrationTests
{
    [TestFixture]
    public class ShowTimeIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private Mock<IApiClientGrpc> _mockApiClientGrpc;
        private int _auditoriumId;

        [SetUp]
        public void Setup()
        {
            _auditoriumId = 2;
            _mockApiClientGrpc = new Mock<IApiClientGrpc>();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<CinemaContext>(options =>
                        options.UseInMemoryDatabase("CinemaDb"));
                    services.AddTransient(_ => _mockApiClientGrpc.Object);
                });
            });
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                context.Database.EnsureDeleted();
            }
            _factory.Dispose();
            _client.Dispose();
        }

        [Test]
        public async Task CreateShowtime_ShowNotFound_ReturnsNotFound()
        {
            var createShowtimeDto = new CreateShowtimeDto { ImdbId = "NonExistentID", SessionDate = DateTime.Now.Date, AuditoriumId = _auditoriumId };

            _mockApiClientGrpc.Setup(client => client.GetById(It.IsAny<string>()))
                .ReturnsAsync((ProtoDefinitions.showResponse)null!);

            var response = await _client.PostAsJsonAsync("/api/v1/showtime", createShowtimeDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var errorResponse = await response.Content.ReadFromJsonAsync<CommonResult<ShowtimeResponseDto>>();
            Assert.That(errorResponse, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(errorResponse.Success, Is.False);
                Assert.That(errorResponse.Message, Is.EqualTo("Show not found"));
            });

        }

        [Test]
        public async Task CreateShowtime_ValidRequest_CreatesShowtimeInDatabase_ReturnsCreated()
        {
            var auditoriumId = 2;
            var createShowtimeDto = new CreateShowtimeDto { ImdbId = "ID-test1", SessionDate = DateTime.Now.Date, AuditoriumId = auditoriumId };
            var expectedMovieTitle = "The mock movie";
            int expectedShowtimeCountBefore = 0;
            int expectedShowtimeCountAfter = 1;

            _mockApiClientGrpc.Setup(client => client.GetById(It.IsAny<string>()))
                .ReturnsAsync(new ProtoDefinitions.showResponse
                {
                    Id = "ID-test1",
                    Title = "The mock movie",
                    Crew = "",
                    Year = "2022"
                });

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                expectedShowtimeCountBefore = context.Showtimes.Count();
            }


            var response = await _client.PostAsJsonAsync("/api/v1/showtime", createShowtimeDto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var returnedShowtime = await response.Content.ReadFromJsonAsync<CommonResult<ShowtimeResponseDto>>();

            Assert.That(returnedShowtime, Is.Not.Null);
            Assert.That(returnedShowtime.Data.MovieTitle, Is.EqualTo(expectedMovieTitle));

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                var createdShowtime = context.Showtimes
                    .Include(x => x.Movie)
                    .Where(s => s.AuditoriumId == auditoriumId && s.Id == returnedShowtime.Data.ShowtimeId).ToList();

                Assert.Multiple(() =>
                {
                    Assert.That(createdShowtime, Has.Count.EqualTo(expectedShowtimeCountAfter));
                    Assert.That(createdShowtime, Is.Not.Null);
                });

                Assert.Multiple(() =>
                {
                    Assert.That(createdShowtime.FirstOrDefault()!.Movie.Title, Is.EqualTo(expectedMovieTitle));
                    Assert.That(createdShowtime.FirstOrDefault()!.SessionDate, Is.EqualTo(createShowtimeDto.SessionDate));
                });

            }
        }
    }
}