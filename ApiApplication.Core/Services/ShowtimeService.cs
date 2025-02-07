using System.Diagnostics;
using System.Text.Json;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Domain.Entities;
using ApiApplication.Infrastructure.Abstractions;
using ApiApplication.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;

namespace ApiApplication.Core.Services
{
    public class ShowtimeService(
        IShowtimesRepository showtimeRepository,
        IApiClientGrpc apiClientGrpc,
        IRedisCacheService redisCacheService,
        ILogger<ShowtimeService> logger) : IShowtimeService
    {
        public async Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeDto createShowtimeDto)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                string cacheKey = $"{createShowtimeDto.ImdbId}";
                ShowResponseDto? show = null;

                var cachedData = await redisCacheService.GetCacheAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    show = JsonSerializer.Deserialize<ShowResponseDto>(cachedData);
                }
                else // cache miss
                {
                    var grpcResponse = await apiClientGrpc.GetById(createShowtimeDto.ImdbId);

                    if (grpcResponse != null)
                    {
                        await redisCacheService.SetCacheAsync(cacheKey, value: JsonSerializer.Serialize(grpcResponse));
                        show = new ShowResponseDto
                        {
                            Id = grpcResponse.Id,
                            Title = grpcResponse.Title,
                            Crew = grpcResponse.Crew,
                            Year = grpcResponse.Year,
                        };
                    }
                }

                if (show == null || string.IsNullOrWhiteSpace(show.Id))
                {
                    throw new Exception("Show not found");
                }

                DateTime showReleaseDate = DateTime.MinValue;

                if (int.TryParse(show.Year, out int year))
                {
                    showReleaseDate = new DateTime(year, 1, 1);
                }

                var showtimeEntity = new ShowtimeEntity
                {
                    SessionDate = createShowtimeDto.SessionDate,
                    Movie = new MovieEntity
                    {
                        Title = show.Title,
                        Stars = show.Crew,
                        ReleaseDate = showReleaseDate,
                        ImdbId = show.Id,
                    },
                    AuditoriumId = createShowtimeDto.AuditoriumId
                };

                var res = await showtimeRepository.CreateShowtime(showtimeEntity, CancellationToken.None);

                var showtimeDto = new ShowtimeResponseDto
                {
                    ShowtimeId = res.Id,
                    MovieTitle = res.Movie.Title,
                    AuditoriumId = res.AuditoriumId,
                    SessionDate = res.SessionDate
                };

                return showtimeDto;
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"CreateShowtimeAsync executed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
