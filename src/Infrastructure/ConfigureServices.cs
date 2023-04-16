using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;

using HotelServices.Infrastructure.Persistence.Models;
using HotelServices.Infrastructure.Persistence.Repositories;
using HotelServices.Infrastructure.Persistence.Services;

using Infrastructure.Persistence.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
            options.DatabaseName     = mongoDbSettings["DatabaseName"]!;
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

        // Configure the mapping for AdditionalService
        ConfigureMongoDbId();
    }

    private static void ConfigureMongoDbId()
    {
        BsonClassMap.RegisterClassMap<AdditionalService>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id)
              .SetIdGenerator(new Int32IdGenerator());
            //cm.MapIdProperty(x => x.Id)
            //    .SetElementName("_id")
            //    .SetIdGenerator(new Int32IdGenerator());
            //    .SetSerializer(new Int32Serializer(BsonType.Int32));
        });

        BsonClassMap.RegisterClassMap<Booking>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id)
              .SetIdGenerator(new Int32IdGenerator());
            //cm.MapIdProperty(x => x.Id)
            //    .SetElementName("_id")
            //    .SetIdGenerator(new Int32IdGenerator())
            //    .SetSerializer(new Int32Serializer(BsonType.Int32));
        });

        BsonClassMap.RegisterClassMap<Image>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id)
              .SetIdGenerator(new Int32IdGenerator());
            //cm.MapIdProperty(x => x.Id)
            //    .SetElementName("_id")
            //    .SetIdGenerator(new Int32IdGenerator())
            //    .SetSerializer(new Int32Serializer(BsonType.Int32));
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