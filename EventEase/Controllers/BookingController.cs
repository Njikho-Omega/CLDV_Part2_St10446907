using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)

        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            if(!string.IsNullOrEmpty(searchString) )
            {
                bookings = bookings.Where(b =>
                    b.Venue.VenueName.Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }
        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewBag.Events = _context.Events.ToList();
                ViewBag.Venues = _context.Venues.ToList();
                return View(booking);
            }

            // Corrected double-booking logic
            var conflict = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
                ViewBag.Events = _context.Events.ToList();
                ViewBag.Venues = _context.Venues.ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "This venue is already booked for that date.");
                    ViewBag.Events = _context.Events.ToList();
                    ViewBag.Venues = _context.Venues.ToList();
                    return View(booking);
                }
            }

            ViewBag.Events = _context.Events.ToList();
            ViewBag.Venues = _context.Venues.ToList();
            return View(booking);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(e => e.Venue)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(m => m.BookingId == id);


            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            
            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();

            return View(booking);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }
        public async Task<IActionResult> ReadOnlyView(string searchString)
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }
    }
}
