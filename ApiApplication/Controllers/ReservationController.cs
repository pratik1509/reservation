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
    public class ReservationController(IReservationService reservationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ReserveSeats(ReserveSeatsDto request)
        {
            var reservation = await reservationService.ReserveSeatsAsync(request, CancellationToken.None);
            return Ok(CommonResult<ReservationResponseDto>.SuccessResult(reservation));
        }
    }
}