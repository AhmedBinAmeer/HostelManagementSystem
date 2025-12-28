using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Student List
        public async Task<IActionResult> Index(string searchString)
        {
            if (HttpContext.Session.GetString("Role") == null) return RedirectToAction("Login", "Account");

            var students = _context.Students.Include(s => s.Room).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.Name.Contains(searchString) || s.StudentRollNo.Contains(searchString));
            }

            return View(await students.ToListAsync());
        }

        // GET: Create Page
        public IActionResult Create()
        {
            ViewBag.Rooms = new SelectList(_context.Rooms, "Id", "RoomNumber");
            return View();
        }

        // ==========================================
        //  UPDATED CREATE METHOD (Checks Capacity)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            ModelState.Remove("Room"); // Remove validation for navigation property

            // 1. CHECK CAPACITY
            if (student.RoomId != null)
            {
                var selectedRoom = await _context.Rooms
                    .Include(r => r.Students) // Load existing students to count them
                    .FirstOrDefaultAsync(r => r.Id == student.RoomId);

                if (selectedRoom != null && selectedRoom.Students.Count >= selectedRoom.Capacity)
                {
                    // Add Error to the specific field
                    ModelState.AddModelError("RoomId", $"Room {selectedRoom.RoomNumber} is full (Capacity: {selectedRoom.Capacity}). Please choose another.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown if validation fails
            ViewBag.Rooms = new SelectList(_context.Rooms, "Id", "RoomNumber", student.RoomId);
            return View(student);
        }

        // GET: Edit Page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewBag.Rooms = new SelectList(_context.Rooms, "Id", "RoomNumber", student.RoomId);
            return View(student);
        }

        // ==========================================
        //  UPDATED EDIT METHOD (Checks Capacity)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            ModelState.Remove("Room");

            // 1. CHECK CAPACITY (Only if moving to a NEW room)
            if (student.RoomId != null)
            {
                // Get the student's CURRENT room from DB to see if they changed it
                var currentStudentInDb = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

                // Only check if the Room ID is DIFFERENT than before
                if (currentStudentInDb.RoomId != student.RoomId)
                {
                    var targetRoom = await _context.Rooms
                        .Include(r => r.Students)
                        .FirstOrDefaultAsync(r => r.Id == student.RoomId);

                    if (targetRoom != null && targetRoom.Students.Count >= targetRoom.Capacity)
                    {
                        ModelState.AddModelError("RoomId", $"Room {targetRoom.RoomNumber} is full. Cannot move student there.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Rooms = new SelectList(_context.Rooms, "Id", "RoomNumber", student.RoomId);
            return View(student);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Room).FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}