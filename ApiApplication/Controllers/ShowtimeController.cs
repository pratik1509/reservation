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
    public class ShowtimeController(IShowtimeService showtimeService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> CreateShowtime(CreateShowtimeDto request)
        {
            var showtime = await showtimeService.CreateShowtimeAsync(request);
            return Ok(CommonResult<ShowtimeResponseDto>.SuccessResult(showtime));
        }
    }
}