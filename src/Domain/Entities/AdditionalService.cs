namespace HotelServices.Domain.Entities;

public class AdditionalService
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public ICollection<Booking> Bookings { get; set; }
    public ICollection<Image> Images { get; set; }
}