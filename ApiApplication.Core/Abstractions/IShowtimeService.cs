using ApiApplication.Core.DTOs;

namespace ApiApplication.Core.Abstractions
{
    public interface IShowtimeService
    {
        Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeDto request);

    }
}