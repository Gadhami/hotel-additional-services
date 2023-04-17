namespace HotelServices.Domain.Entities.DTO;

public class BookingDTO
{
    public string?  Id    { get; set; }
    public DateTime Start { get; set; }
    public DateTime End   { get; set; }
}