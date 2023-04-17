using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;

namespace HotelServices.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(
            IRepository<AdditionalService> servicesRepo,
            IRepository<Booking> bookingsRepo,
            IRepository<Image> imagesRepo
        )
    {
        AdditionalServices = servicesRepo;
        Bookings           = bookingsRepo;
        Images             = imagesRepo;
    }

    public IRepository<AdditionalService> AdditionalServices { get; }
    public IRepository<Booking> Bookings { get; }
    public IRepository<Image> Images { get; }

    public IRepository<T> Tables<T>() where T : class
    {
        return typeof(T) switch
        {
            var _ when typeof(T) == typeof(AdditionalService) => (IRepository<T>)AdditionalServices,
            var _ when typeof(T) == typeof(Booking) => (IRepository<T>)Bookings,
            var _ when typeof(T) == typeof(Image) => (IRepository<T>)Images,

            _ => throw new ArgumentException("You must provide a valid table model object (AdditionalService, Booking, ...)")
        };
    }

    #region Dispose Code

    public void Dispose()
    {
        Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion Dispose Code
}