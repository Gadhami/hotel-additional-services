using HotelServices.Domain.Interfaces;

namespace HotelServices.Domain.Entities;

public class AdditionalService : IEntity
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; }
    public decimal Price { get; set; }
    public ICollection<Booking> Bookings { get; set; }
    public ICollection<Image> Images { get; set; }
}