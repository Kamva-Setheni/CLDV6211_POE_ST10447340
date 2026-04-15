using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Venue")]
        public int? VenueID { get; set; }

        [Display(Name = "Event Type")]
        public string? EventType { get; set; } = "General";

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        public virtual Venue? Venue { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}