namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
