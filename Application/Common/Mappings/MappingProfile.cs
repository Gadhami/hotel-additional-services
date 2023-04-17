using AutoMapper;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;

namespace HotelServices.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AdditionalService, AdditionalServiceDTO>().ReverseMap();
            CreateMap<Booking, BookingDTO>().ReverseMap();
            CreateMap<Image, ImageDTO>().ReverseMap();
        }
    }
}