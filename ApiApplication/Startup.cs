using ApiApplication.Core.Abstractions;
using ApiApplication.Core.Services;
using ApiApplication.Database;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Infrastructure.Abstractions;
using ApiApplication.Infrastructure.Persistence;
using ApiApplication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using ApiApplication.Infrastructure.ExternalServices;
using ApiApplication.Middlewares;
using System;
using System.Threading;

namespace ApiApplication
{
    public class Startup(IConfiguration configuration)
    {
        private const int MaxRetries = 50;
        private const int RetryDelayMilliseconds = 2000;
        public IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
            services.AddControllers();

            services.AddScoped<IShowtimeService, ShowtimeService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IPurchaseService, PurchaseService>();

            string redisConnectionString = Configuration.GetValue<string>("Redis:ConnectionString");
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                Console.WriteLine("✅ Connected to Redis successfully.");
                return redis;
            });
            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new UrlSegmentApiVersionReader()
                );
            });

            // **Versioned API Explorer (for Swagger)**
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            // **Register ApiClientGrpc**
            services.AddSingleton<IApiClientGrpc, ApiClientGrpc>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<GlobalExceptionHandler>();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; // This makes Swagger available at the root URL
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SampleData.Initialize(app);
        }
    }
}
