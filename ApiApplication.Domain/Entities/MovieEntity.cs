﻿namespace ApiApplication.Domain.Entities
{
    public class MovieEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ImdbId { get; set; } = "";
        public string Stars { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public List<ShowtimeEntity> Showtimes { get; set; } = [];
    }
}
