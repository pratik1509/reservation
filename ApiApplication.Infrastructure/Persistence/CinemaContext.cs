using ApiApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiApplication.Database
{
    public class CinemaContext(DbContextOptions<CinemaContext> options) : DbContext(options)
    {
        public DbSet<AuditoriumEntity> Auditoriums { get; set; }
        public DbSet<ShowtimeEntity> Showtimes { get; set; }
        public DbSet<MovieEntity> Movies { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditoriumEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
                build.HasMany(entry => entry.Showtimes)
                     .WithOne()
                     .HasForeignKey(entity => entity.AuditoriumId)
                     .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SeatEntity>(build =>
            {
                build.HasKey(entry => new { entry.AuditoriumId, entry.Row, entry.SeatNumber });

                build.HasOne(entry => entry.Auditorium)
                     .WithMany(entry => entry.Seats)
                     .HasForeignKey(entry => entry.AuditoriumId)
                     .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShowtimeEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();

                // Explicitly define the foreign key for MovieEntity
                build.HasOne(entry => entry.Movie)
                     .WithMany(entry => entry.Showtimes)
                     .HasForeignKey("MovieId") // This ensures EF creates the MovieId field
                     .IsRequired()
                     .OnDelete(DeleteBehavior.Cascade);

                build.HasMany(entry => entry.Tickets)
                     .WithOne(entry => entry.Showtime)
                     .HasForeignKey(entry => entry.ShowtimeId)
                     .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MovieEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
                build.Property(entry => entry.Title)
                     .IsRequired()
                     .HasMaxLength(255);
                build.Property(entry => entry.ImdbId)
                     .HasMaxLength(20);
                build.Property(entry => entry.Stars)
                     .HasMaxLength(500);
            });

            modelBuilder.Entity<TicketEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();

                build.HasOne(entry => entry.Showtime)
                     .WithMany(entry => entry.Tickets)
                     .HasForeignKey(entry => entry.ShowtimeId)
                     .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
