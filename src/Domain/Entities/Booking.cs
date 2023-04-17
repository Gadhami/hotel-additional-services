using HotelServices.Domain.Interfaces;

namespace HotelServices.Domain.Entities;

public class Booking : IEntity
{
    public string   Id    { get; set; } = null!;
    public DateTime Start { get; set; } = DateTime.UtcNow.AddHours(1);
    public DateTime End   { get; set; } = DateTime.UtcNow.AddHours(2);
}