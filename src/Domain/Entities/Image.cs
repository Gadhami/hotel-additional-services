using HotelServices.Domain.Interfaces;

namespace HotelServices.Domain.Entities;

public class Image : IEntity
{
    public string Id   { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string URL  { get; set; } = null!;
}