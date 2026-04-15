using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor
        public VenueController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Venue
        // Displays all venues with optional search functionality
        public async Task<IActionResult> Index(string searchString)
        {
            // Get all venues from database
            var venues = _context.Venue.AsQueryable();

            // Apply search filter if search term is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString) ||
                                          v.Location.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            // Include related events to check for associations
            var venueList = await venues.Include(v => v.Events).ToListAsync();

            // Display success/error messages if they exist
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];
                        return View(venueList);
        }

        // GET: Venue/Create
        // Displays form to create a new venue
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        // Creates a new venue in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            // Remove validation for auto-generated ID
            ModelState.Remove("VenueID");
            ModelState.Remove("Events");

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default placeholder image if none provided
                    if (string.IsNullOrEmpty(venue.ImageUrl))
                    {
                        venue.ImageUrl = "https://picsum.photos/id/106/400/300";
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Venue '{venue.VenueName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save venue. A venue with this name may already exist.");
                }
            }

            return View(venue);
        }

        // GET: Venue/Edit/5
        // Displays form to edit an existing venue
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Venue ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            var venue = await _context.Venue
                .Include(v => v.Events)
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null)
            {
                TempData["Error"] = $"Venue with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // POST: Venue/Edit/5
        // Updates an existing venue in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            if (id != venue.VenueID)
            {
                TempData["Error"] = "Venue ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("Events");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Venue '{venue.VenueName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID))
                    {
                        TempData["Error"] = "Venue no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Concurrency error. Please try again.");
                    }
                }
            }

            return View(venue);
        }

        // GET: Venue/Delete/5
        // Displays confirmation page for deleting a venue
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Venue ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            var venue = await _context.Venue
                .Include(v => v.Events)
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null)
            {
                TempData["Error"] = $"Venue with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            // BUSINESS RULE: Prevent deletion if venue has associated events
            if (venue.Events != null && venue.Events.Any())
            {
                TempData["Error"] = $"Cannot delete venue '{venue.VenueName}' because it has {venue.Events.Count} associated event(s). Delete the events first.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // POST: Venue/Delete/5
        // Actually deletes the venue from database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue != null)
            {
                // Double-check no events exist before deletion
                bool hasEvents = await _context.Event.AnyAsync(e => e.VenueID == id);
                if (hasEvents)
                {
                    TempData["Error"] = $"Cannot delete venue '{venue.VenueName}' - it has associated events.";
                    return RedirectToAction(nameof(Index));
                }

                string venueName = venue.VenueName;
                _context.Venue.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Venue '{venueName}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Venue/Details/5
        // Shows detailed information about a specific venue
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Venue ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            var venue = await _context.Venue
                .Include(v => v.Events)
                    .ThenInclude(e => e.Bookings) // Include bookings for statistics
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null)
            {
                TempData["Error"] = $"Venue with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            // Calculate statistics for display
            ViewBag.TotalEvents = venue.Events?.Count ?? 0;
            ViewBag.TotalBookings = venue.Events?.Sum(e => e.Bookings?.Count ?? 0) ?? 0;

            return View(venue);
        }

        // Helper method to check if a venue exists
        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}