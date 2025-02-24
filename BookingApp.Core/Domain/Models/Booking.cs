using BookingApp.Core.Domain.Models;

namespace BookingApp.Core.Domain.Models
{
    public class Booking : BaseModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int InventoryId { get; set; }
        public DateTime BookingDate { get; set; }

        public Member Member { get; set; }

        public Inventory Inventory { get; set; }

    }
}
