using System.ComponentModel.DataAnnotations;
using MediatR;

namespace HotelDemo.Mediator.Queries.Bookings
{
    public class CalculateBookingCostQuery : IRequest<decimal>
    {
        [Required]
        public long RoomId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfGuests { get; set; }

        [Required]
        public bool IncludeBreakfast { get; set; }
    }
}
