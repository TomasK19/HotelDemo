

using System.ComponentModel.DataAnnotations;

using HotelDemo.DTO;

using MediatR;

namespace HotelDemo.Mediator.Commands.Bookings
{
    public class CreateBookingCommand : IRequest<BookingDto>
    {

        [Required]
        public long HotelId { get; set; }
        [Required]
        public long RoomId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Range(1, int.MaxValue)]
        public int NumberOfGuests { get; set; }
        public bool IncludeBreakfast { get; set; }
        public decimal TotalCost { get; set; }
        public long UserId { get; set; }
    }
}