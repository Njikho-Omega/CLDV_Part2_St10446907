namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; }
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}
