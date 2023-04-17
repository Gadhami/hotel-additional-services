using AutoMapper;
using HotelServices.Application.Common.Mappings;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigAutoMapper
{
    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        var MapperConfig = new MapperConfiguration(ConfigExpr =>
        {
            ConfigExpr.AddProfile(new MappingProfile());
        });

        var mapper       = MapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }
}