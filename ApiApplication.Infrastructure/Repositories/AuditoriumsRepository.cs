using ApiApplication.Database;
using ApiApplication.Domain.Entities;
using ApiApplication.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ApiApplication.Infrastructure.Repositories
{
    public class AuditoriumsRepository(CinemaContext context) : IAuditoriumsRepository
    {
        public async Task<AuditoriumEntity?> GetAsync(int auditoriumId, CancellationToken cancel)
        {
            return await context.Auditoriums
                .Include(x => x.Seats)
                .FirstOrDefaultAsync(x => x.Id == auditoriumId, cancel);
        }
    }
}
