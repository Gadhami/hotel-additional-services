namespace HotelServices.Domain.Entities.DTO;

public class AdditionalServiceDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public ICollection<BookingDTO> Bookings { get; set; }
    public ICollection<ImageDTO> Images { get; set; }
}