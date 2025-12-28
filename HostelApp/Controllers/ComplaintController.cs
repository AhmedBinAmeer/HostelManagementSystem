using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List of Complaints (Warden View)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") == null) return RedirectToAction("Login", "Account");

            // Get complaints with Student and Room details
            var complaints = await _context.Complaints
                                           .Include(c => c.Student)
                                           .ThenInclude(s => s.Room)
                                           .OrderByDescending(c => c.Date) // Newest first
                                           .ToListAsync();
            return View(complaints);
        }

        // POST: Mark as Resolved
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.Status = "Resolved";
                _context.Update(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}