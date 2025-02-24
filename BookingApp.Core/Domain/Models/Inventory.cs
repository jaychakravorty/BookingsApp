using BookingApp.Core.Domain.Models;

namespace BookingApp.Core.Domain.Models
{
    public class Inventory : BaseModel
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public int RemainingInventory { get; set; }

        public DateOnly ExpirationDate { get; set; }

        //public ICollection<Booking> Bookings { get; set; }
    }
}
