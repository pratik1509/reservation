using ApiApplication.Core.DTOs;
using ApiApplication.Database;
using ApiApplication.Domain.Entities;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace ApiApplication.IntegrationTests
{
    [TestFixture]
    public class PurchaseIntegrationTest
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

        private async Task SeedDataAsync(CinemaContext context, Guid reservationId, bool isPaid, DateTime createdTime)
        {
            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 4,
                Showtimes = new List<ShowtimeEntity>
                {
                    new() {
                        Id = 2,
                        SessionDate = new DateTime(2023, 1, 1),
                        Movie = new MovieEntity
                        {
                            Id = 2,
                            Title = "Inception",
                            ImdbId = "tt1375666",
                            ReleaseDate = new DateTime(2010, 01, 14),
                            Stars = "Leonardo DiCaprio, Joseph Gordon-Levitt, Ellen Page, Ken Watanabe"
                        },
                        AuditoriumId = 4,
                        Tickets = [
                            new TicketEntity {
                                Id = reservationId,
                                ShowtimeId = 2,
                                Seats = new List<SeatEntity> {
                                    new SeatEntity { Row = 1, SeatNumber = 1 },
                                    new SeatEntity { Row = 1, SeatNumber = 2 }
                                },
                                CreatedTime = createdTime,
                                Paid = isPaid
                            }
                        ],
                    }
                },
                Seats = new List<SeatEntity>() // Assuming GenerateSeats is not available
            });

            await context.SaveChangesAsync();
        }

        [Test]
        public async Task BuySeats_ReturnsSuccess_WhenReservationIsValid()
        {
            var reservationId = Guid.NewGuid();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                await SeedDataAsync(context, reservationId, false, DateTime.Now);
            }

            var response = await _client.PostAsync($"/api/v1/Purchase/{reservationId}", null);
            var result = await response.Content.ReadFromJsonAsync<CommonResult<PurchaseResponseDto>>();

            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Data.Success, Is.True);
                Assert.That(result?.Data.Message, Is.EqualTo("Reservation confirmed successfully."));
            });
        }

        [Test]
        public async Task BuySeats_ReturnsError_WhenReservationHasExpired()
        {
            var reservationId = Guid.NewGuid();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                await SeedDataAsync(context, reservationId, false, DateTime.Now.AddMinutes(-10));
            }

            var response = await _client.PostAsync($"/api/v1/Purchase/{reservationId}", null);
            var result = await response.Content.ReadFromJsonAsync<CommonResult<PurchaseResponseDto>>();

            Assert.That(result?.Data.Success, Is.False);
            Assert.That(result?.Data.Message, Is.EqualTo("Reservation has expired."));
        }

        [Test]
        public async Task BuySeats_ReturnsError_WhenReservationDoesNotExist()
        {
            var reservationId = Guid.NewGuid();
            var response = await _client.PostAsync($"/api/v1/Purchase/{reservationId}", null);
            var result = await response.Content.ReadFromJsonAsync<CommonResult<PurchaseResponseDto>>();

            Assert.That(result?.Data.Success, Is.False);
            Assert.That(result?.Data.Message, Is.EqualTo("Reservation not found."));
        }

        [Test]
        public async Task BuySeats_ReturnsError_WhenTicketsAlreadyPaid()
        {
            var reservationId = Guid.NewGuid();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
                await SeedDataAsync(context, reservationId, true, DateTime.Now);
            }

            var response = await _client.PostAsync($"/api/v1/Purchase/{reservationId}", null);
            var result = await response.Content.ReadFromJsonAsync<CommonResult<PurchaseResponseDto>>();

            Assert.That(result?.Data.Success, Is.False);
            Assert.That(result?.Data.Message, Is.EqualTo("Tickets already paid."));
        }
    }
}
