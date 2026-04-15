using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Fix the constructor
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    (b.Event != null && b.Event.EventName.Contains(searchString)) ||
                    (b.CustomerName != null && b.CustomerName.Contains(searchString)));
                ViewData["CurrentFilter"] = searchString;
            }

            var bookingList = await bookings.ToListAsync();

            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(bookingList);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            var availableEvents = _context.Event
                .Include(e => e.Venue)
                .Where(e => e.EventDate >= DateTime.Today)
                .ToList();

            ViewBag.Events = availableEvents;
            return View();
        }

        // Helper method
        private string GenerateBookingReference()
        {
            string reference;
            bool isUnique;

            do
            {
                reference = $"EVT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                isUnique = !_context.Booking.Any(b => b.BookingReference == reference);
            } while (!isUnique);

            return reference;
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }
    }
}