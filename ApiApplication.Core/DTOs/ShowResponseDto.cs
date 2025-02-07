namespace ApiApplication.Core.DTOs
{
    public class ShowResponseDto
    {
        public string Id { get; set; } = "";
        public string Rank { get; set; } = "";
        public string Title { get; set; } = "";
        public string FullTitle { get; set; } = "";
        public string Year { get; set; } = "";
        public string Image { get; set; } = "";
        public string Crew { get; set; } = "";
        public string ImDbRating { get; set; } = "";
        public string ImDbRatingCount { get; set; } = "";
    }

    public class ShowListResponseDto
    {
        public List<ShowResponseDto> Shows { get; set; } = [];
    }
}