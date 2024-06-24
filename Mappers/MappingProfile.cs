using AutoMapper;

using HotelDemo.DTO;
using HotelDemo.Mediator.Commands.Bookings;
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

        CreateMap<CreateBookingCommand, Booking>()
            .ForMember(dest => dest.Hotel, opt => opt.Ignore())
            .ForMember(dest => dest.Room, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfNights, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCost, opt => opt.Ignore());

    }
}
