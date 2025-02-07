using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ApiApplication.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiApplication.Middlewares
{
    public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred.");

                var response = new CommonResult<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}