using System.ComponentModel.DataAnnotations;

namespace BookingApp.API.Models
{
    public record BookingDetails
    {
        [Required]
        public string MemberName { get; set; }
        [Required]
        public string MemberSurname { get; set; }
        [Required]
        public string InventoryName { get; set; }

    }
}
