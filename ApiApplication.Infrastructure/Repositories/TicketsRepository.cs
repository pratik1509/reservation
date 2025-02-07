using ApiApplication.Database;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiApplication.Infrastructure.Repositories
{
    public class TicketsRepository(CinemaContext context) : ITicketsRepository
    {
        public Task<TicketEntity?> GetAsync(Guid id, CancellationToken cancel)
        {
            return context.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancel);
        }

        public async Task<IEnumerable<TicketEntity>> GetEnrichedAsync(int showtimeId, CancellationToken cancel)
        {
            return await context.Tickets
                .Include(x => x.Showtime)
                .Include(x => x.Seats)
                .Where(x => x.ShowtimeId == showtimeId)
                .ToListAsync(cancel);
        }

        public async Task<TicketEntity> CreateAsync(ShowtimeEntity showtime, IEnumerable<SeatEntity> selectedSeats, CancellationToken cancel)
        {
            var ticket = context.Tickets.Add(new TicketEntity
            {
                Showtime = showtime,
                Seats = [.. selectedSeats]
            });

            await context.SaveChangesAsync(cancel);

            return ticket.Entity;
        }

        public async Task<TicketEntity> ConfirmPaymentAsync(TicketEntity ticket, CancellationToken cancel)
        {
            ticket.Paid = true;
            context.Update(ticket);
            await context.SaveChangesAsync(cancel);
            return ticket;
        }
    }
}
