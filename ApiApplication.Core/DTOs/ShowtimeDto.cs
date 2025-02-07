namespace ApiApplication.Core.DTOs
{
    // DTO to receive input data for creating a showtime
    public class CreateShowtimeDto
    {
        public required string ImdbId { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }

    // DTO for returning the details of the created showtime
    public class ShowtimeResponseDto
    {
        public int ShowtimeId { get; set; }
        public string? MovieTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
