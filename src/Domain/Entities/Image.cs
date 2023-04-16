namespace HotelServices.Domain.Entities;

public class Image
{
    public int    Id   { get; set; }
    public string Name { get; set; } = null!;
    public string URL  { get; set; } = null!;
}