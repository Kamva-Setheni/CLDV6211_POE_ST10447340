using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required]
        [Display(Name = "Event")]
        public int EventID { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Display(Name = "Booking Reference")]
        public string? BookingReference { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }

        [Phone]
        [Display(Name = "Customer Phone")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string? Status { get; set; } = "Confirmed";

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        // Navigation properties
       

        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }

        [ForeignKey("VenueID")]
        public virtual Venue? Venue { get; set; }
        // Note: Venue is accessed through Event.Venue, not directly here
    }
}