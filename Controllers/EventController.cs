using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Fix the constructor - properly assign the parameter
        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Event
        public async Task<IActionResult> Index(string searchString, string eventType)
        {
            var events = _context.Event
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString) ||
                                          (e.Description != null && e.Description.Contains(searchString)));
                ViewData["CurrentFilter"] = searchString;
            }

            var eventList = await events.ToListAsync();

            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(eventList);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,VenueID,EventType,ImageUrl")] Event eventModel)
        {
            ModelState.Remove("EventID");
            ModelState.Remove("Venue");
            ModelState.Remove("Bookings");

            if (eventModel.VenueID.HasValue)
            {
                bool isVenueBooked = await _context.Event
                    .AnyAsync(e => e.VenueID == eventModel.VenueID &&
                                   e.EventDate.Date == eventModel.EventDate.Date);

                if (isVenueBooked)
                {
                    ModelState.AddModelError("VenueID", "This venue is already booked for the selected date.");
                }
            }

            if (eventModel.EventDate < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "Event date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(eventModel.ImageUrl))
                    {
                        eventModel.ImageUrl = "https://picsum.photos/id/292/400/300";
                    }
                    if (string.IsNullOrEmpty(eventModel.EventType))
                    {
                        eventModel.EventType = "General";
                    }

                    _context.Add(eventModel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Event '{eventModel.EventName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save event.");
                }
            }

            ViewBag.Venues = _context.Venue.ToList();
            return View(eventModel);
        }

        // Add other methods (Edit, Delete, Details, etc.) with the same pattern
        // Make sure all methods use _context which is now properly initialized

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}