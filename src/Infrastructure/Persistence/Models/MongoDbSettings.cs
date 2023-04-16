namespace HotelServices.Infrastructure.Persistence.Models;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}