using AutoMapper;
using HotelDemo.DTO;
using HotelDemo.Models;

namespace HotelDemo.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();

        CreateMap<Hotel, HotelDto>().ReverseMap();
        CreateMap<Room, RoomDto>().ReverseMap();
        CreateMap<RoomPicture, RoomPictureDto>().ReverseMap();
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.Hotel, opt => opt.Condition(src => src.Hotel != null))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Room.Type))
            .ReverseMap();
        CreateMap<BookingCostCalculationDto, Booking>()
            .ForMember(
                dest => dest.NumberOfNights,
                opt => opt.MapFrom(src => (src.EndDate - src.StartDate).TotalDays)
            );
    }
}
