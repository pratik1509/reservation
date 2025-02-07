using ApiApplication.Core.DTOs;
using ApiApplication.Database;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace ApiApplication.IntegrationTests
{
    [TestFixture]
    public class ReservationIntegrationTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<CinemaContext>(options =>
                        options.UseInMemoryDatabase("CinemaDb"));
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
        public async Task ReserveSeats_ReturnsSuccess_WhenSeatsAreAvailable()
        {
            var request = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }, new SeatDto { RowNumber = 1, SeatNumber = 2 }]
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Reservation", request);
            var result = await response.Content.ReadFromJsonAsync<CommonResult<ReservationResponseDto>>();

            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(result, Is.Not.Null);
            });

            Assert.That(result?.Data.ReservationId, Is.Not.Null);
        }

        [Test]
        public async Task ReserveSeats_ReturnsError_WhenShowtimeDoesNotExist()
        {
            var request = new ReserveSeatsDto
            {
                ShowtimeId = 999,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }]
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Reservation", request);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task ReserveSeats_ReturnsError_WhenSeatsAreUnavailable()
        {
            var firstRequest = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }]
            };
            await _client.PostAsJsonAsync("/api/v1/Reservation", firstRequest);

            var secondRequest = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }]
            };
            var response = await _client.PostAsJsonAsync("/api/v1/Reservation", secondRequest);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task ReserveSeats_ReturnsError_WhenSeatsAreNotContiguous()
        {
            var request = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }, new SeatDto { RowNumber = 1, SeatNumber = 3 }]
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Reservation", request);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        public async Task ReserveSeats_ReturnsError_WhenAuditoriumDoesNotExist()
        {
            var request = new ReserveSeatsDto
            {
                ShowtimeId = 1,
                AuditoriumId = 999,
                Seats = [new SeatDto { RowNumber = 1, SeatNumber = 1 }]
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Reservation", request);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }
    }
}