using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  UPDATED INDEX METHOD (Supports Filter by Floor)
        // ============================================================
        public async Task<IActionResult> Index(string floor)
        {
            // Security Check
            if (HttpContext.Session.GetString("Role") == null) return RedirectToAction("Login", "Account");

            // 1. Fetch unique floors for the dropdown list (e.g., "B1", "G", "F1")
            ViewBag.Floors = await _context.Rooms
                                           .Select(r => r.Floor)
                                           .Distinct()
                                           .OrderBy(f => f)
                                           .ToListAsync();

            // Keep the selected floor active in the dropdown
            ViewBag.SelectedFloor = floor;

            // 2. Start the query (Include students to count occupancy)
            var query = _context.Rooms.Include(r => r.Students).AsQueryable();

            // 3. Apply Filter if the user selected a floor
            if (!string.IsNullOrEmpty(floor))
            {
                query = query.Where(r => r.Floor == floor);
            }

            // 4. Return the filtered list
            return View(await query.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            // 1. Ignore the Students list for validation
            ModelState.Remove("Students");

            // 2. CHECK FOR DUPLICATE: Look for any room with the same number
            bool isDuplicate = await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber);

            if (isDuplicate)
            {
                // 3. Add a custom error message to the "RoomNumber" field
                ModelState.AddModelError("RoomNumber", "A room with this number already exists. Please choose a unique name.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(room); //
                await _context.SaveChangesAsync(); //
                return RedirectToAction(nameof(Index)); //
            }

            // 4. If duplicate was found, it returns to the form and shows the error
            return View(room); //
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: Edit
        // POST: Edit Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            // CRITICAL FIX: Ignore the "Students" list validation check
            ModelState.Remove("Students");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == room.Id))
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

            // If we get here, validation failed. Return view to show errors.
            return View(room);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Check if any students are currently assigned to this room
            var hasStudents = await _context.Students.AnyAsync(s => s.RoomId == id);

            if (hasStudents)
            {
                // 2. If students exist, set an error message and redirect back to Index
                TempData["Error"] = "Cannot delete this room because students are currently assigned to it. Please move the students to another room first.";
                return RedirectToAction(nameof(Index));
            }

            // 3. If room is empty, proceed with deletion
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Room deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}