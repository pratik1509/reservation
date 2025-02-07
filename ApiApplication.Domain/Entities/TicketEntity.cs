﻿namespace ApiApplication.Domain.Entities
{
    public class TicketEntity
    {
        public TicketEntity()
        {
            CreatedTime = DateTime.Now;
            Paid = false;
        }

        public Guid Id { get; set; }
        public int ShowtimeId { get; set; }
        public ICollection<SeatEntity> Seats { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Paid { get; set; }
        public ShowtimeEntity Showtime { get; set; }
    }
}
