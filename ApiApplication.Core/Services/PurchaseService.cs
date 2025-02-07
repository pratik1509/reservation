using System.Diagnostics;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

public class PurchaseService(ITicketsRepository ticketsRepository, ILogger<PurchaseService> logger) : IPurchaseService
{
    public async Task<PurchaseResponseDto> BuySeatsAsync(BuySeatsDto request, CancellationToken cancel)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var ticket = await ticketsRepository.GetAsync(request.ReservationId, cancel);
            if (ticket == null)
            {
                return new PurchaseResponseDto
                {
                    Success = false,
                    Message = "Reservation not found."
                };
            }

            if (DateTime.Now > ticket.CreatedTime.AddMinutes(10))
            {
                return new PurchaseResponseDto
                {
                    Success = false,
                    Message = "Reservation has expired."
                };
            }

            if (ticket.Paid)
            {
                return new PurchaseResponseDto
                {
                    Success = false,
                    Message = "Tickets already paid."
                };
            }

            await ticketsRepository.ConfirmPaymentAsync(ticket, cancel);

            return new PurchaseResponseDto
            {
                Success = true,
                Message = "Reservation confirmed successfully."
            };
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation($"BuySeatsAsync executed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
