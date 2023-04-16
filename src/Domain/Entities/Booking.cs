namespace HotelServices.Domain.Entities;

public class Booking
{
    public int      Id    { get; set; }
    public DateTime Start { get; set; } = DateTime.UtcNow.AddHours(1);
    public DateTime End   { get; set; } = DateTime.UtcNow.AddHours(2);
}