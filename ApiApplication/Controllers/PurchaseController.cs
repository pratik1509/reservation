using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Core.Abstractions;
using ApiApplication.Core.DTOs;
using ApiApplication.Results;
using Microsoft.AspNetCore.Mvc;

namespace ApiApplication.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PurchaseController(IPurchaseService purchaseService) : ControllerBase
    {
        [HttpPost("{reservationId}")]
        public async Task<IActionResult> BuySeats(Guid reservationId)
        {
            var result = await purchaseService.BuySeatsAsync(new BuySeatsDto { ReservationId = reservationId }, CancellationToken.None);
            return Ok(CommonResult<PurchaseResponseDto>.SuccessResult(result));
        }
    }
}