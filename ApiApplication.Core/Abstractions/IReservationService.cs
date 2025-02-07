using ApiApplication.Core.DTOs;
using ApiApplication.Domain.Entities;

namespace ApiApplication.Core.Abstractions
{
    public interface IReservationService
    {
        Task<ReservationResponseDto> ReserveSeatsAsync(ReserveSeatsDto request, CancellationToken cancel);
    }
}