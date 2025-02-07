namespace ApiApplication.Core.DTOs
{
    public class BuySeatsDto
    {
        public Guid ReservationId { get; set; }
    }

    public class PurchaseResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}