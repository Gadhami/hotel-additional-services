using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;

using HotelServices.Infrastructure.Persistence.Models;
using HotelServices.Infrastructure.Persistence.Repositories;

using Infrastructure.Persistence.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDB(configuration);
        services.SetupRepositoryDI();

        return services;
    }

    private static void ConfigureDB(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetSection("MongoDbSettings");
        services.Configure<MongoDbSettings>(options =>
        {
            options.ConnectionString = mongoDbSettings["ConnectionString"]!;
            options.DatabaseName = mongoDbSettings["DatabaseName"]!;
        });

        services.AddSingleton<IMongoClient>(s =>
        {
            var settings = s.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton(s =>
        {
            var client = s.GetRequiredService<IMongoClient>();
            var settings = s.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            return client.GetDatabase(settings.DatabaseName);
        });
    }

    private static void SetupRepositoryDI(this IServiceCollection services)
    {
        services.AddScoped<IRepository<AdditionalService>, MongoDbRepository<AdditionalService>>();
        services.AddScoped<IRepository<Booking>, MongoDbRepository<Booking>>();
        services.AddScoped<IRepository<Image>, MongoDbRepository<Image>>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}