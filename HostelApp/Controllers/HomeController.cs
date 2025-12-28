using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
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
            // Security Check: If not logged in, go to Login
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Dashboard Stats Logic
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();

            // Calculate available beds
            var rooms = await _context.Rooms.Include(r => r.Students).ToListAsync();
            int totalCapacity = rooms.Sum(r => r.Capacity);
            int occupiedBeds = rooms.Sum(r => r.Students.Count);
            ViewBag.EmptyBeds = totalCapacity - occupiedBeds;

            // Pending Complaints
            ViewBag.PendingComplaints = await _context.Complaints.CountAsync(c => c.Status == "Pending");

            return View();
        }
    }
}