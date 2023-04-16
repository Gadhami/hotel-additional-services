using HotelServices.Domain.Entities;

namespace HotelServices.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<AdditionalService> AdditionalServices { get; }
    IRepository<Booking> Bookings { get; }
    IRepository<Image> Images { get; }

    IRepository<T> Tables<T>() where T : class;
}