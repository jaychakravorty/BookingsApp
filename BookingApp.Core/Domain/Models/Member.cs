namespace BookingApp.Core.Domain.Models
{
    public class Member : BaseModel
    {
        
        public int Id { get; set; }
        public string Name { get; set; }

        public string Surname { get; set; }
        public int BookingCount { get; set; } 

        public DateTimeOffset DateJoined { get; set; }

        //public ICollection<Booking> Bookings { get; set; }
    }
}
