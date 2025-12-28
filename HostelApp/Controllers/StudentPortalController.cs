using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class StudentPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to check if student is logged in
        private int? GetStudentId()
        {
            return HttpContext.Session.GetInt32("StudentId");
        }

        public async Task<IActionResult> Dashboard()
        {
            var studentId = GetStudentId();
            if (studentId == null) return RedirectToAction("Login", "Account");

            // 1. Fetch Student + Room Info
            var student = await _context.Students
                .Include(s => s.Room)
                .ThenInclude(r => r.Students) // Get roommates too
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return RedirectToAction("Login", "Account");

            // 2. Fetch My Fees
            ViewBag.MyFees = await _context.FeeChallans
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            // 3. Fetch My Complaints
            ViewBag.MyComplaints = await _context.Complaints
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(student);
        }

        // POST: Submit a Complaint
        [HttpPost]
        public async Task<IActionResult> SubmitComplaint(string title, string description)
        {
            var studentId = GetStudentId();
            if (studentId != null)
            {
                var complaint = new Complaint
                {
                    StudentId = studentId.Value,
                    Title = title,
                    Description = description,
                    Date = DateTime.Now,
                    Status = "Pending"
                };
                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Complaint submitted successfully!";
            }
            return RedirectToAction("Dashboard");
        }
    }
}