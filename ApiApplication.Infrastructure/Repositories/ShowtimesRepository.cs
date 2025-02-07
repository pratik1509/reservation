using System.Linq.Expressions;
using ApiApplication.Database;
using ApiApplication.Domain.Entities;
using ApiApplication.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ApiApplication.Infrastructure.Repositories
{
    public class ShowtimesRepository(CinemaContext context) : IShowtimesRepository
    {
        public async Task<ShowtimeEntity?> GetWithMoviesByIdAsync(int id, CancellationToken cancel)
        {
            return await context.Showtimes
                .Include(x => x.Movie)
                .FirstOrDefaultAsync(x => x.Id == id, cancel);
        }

        public async Task<ShowtimeEntity?> GetWithTicketsByIdAsync(int id, CancellationToken cancel)
        {
            return await context.Showtimes
                .Include(x => x.Tickets)
                .FirstOrDefaultAsync(x => x.Id == id, cancel);
        }

        public async Task<IEnumerable<ShowtimeEntity>> GetAllAsync(Expression<Func<ShowtimeEntity, bool>> filter, CancellationToken cancel)
        {
            if (filter == null)
            {
                return await context.Showtimes
                .Include(x => x.Movie)
                .ToListAsync(cancel);
            }
            return await context.Showtimes
                .Include(x => x.Movie)
                .Where(filter)
                .ToListAsync(cancel);
        }

        public async Task<ShowtimeEntity> CreateShowtime(ShowtimeEntity showtimeEntity, CancellationToken cancel)
        {
            var showtime = await context.Showtimes.AddAsync(showtimeEntity, cancel);
            await context.SaveChangesAsync(cancel);
            return showtime.Entity;
        }
    }
}
