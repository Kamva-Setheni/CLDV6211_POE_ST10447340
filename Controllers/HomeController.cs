using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // FIXED: All await statements have proper spacing and parentheses
                ViewBag.VenueCount = await _context.Venue.CountAsync();
                ViewBag.EventCount = await _context.Event.CountAsync();
                ViewBag.BookingCount = await _context.Booking.CountAsync();

                ViewBag.FeaturedEvents = await _context.Event
                    .Include(e => e.Venue)
                    .Where(e => e.EventDate >= DateTime.Now)
                    .OrderBy(e => e.EventDate)
                    .Take(3)
                    .ToListAsync();  // FIXED: Changed ToListAsyncO to ToListAsync()
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                ViewBag.VenueCount = 0;
                ViewBag.EventCount = 0;
                ViewBag.BookingCount = 0;
                ViewBag.FeaturedEvents = new List<Event>();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}