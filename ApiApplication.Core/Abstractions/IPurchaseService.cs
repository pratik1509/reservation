using ApiApplication.Core.DTOs;

namespace ApiApplication.Core.Abstractions
{
    public interface IPurchaseService
    {
        Task<PurchaseResponseDto> BuySeatsAsync(BuySeatsDto request, CancellationToken cancel);
    }
}