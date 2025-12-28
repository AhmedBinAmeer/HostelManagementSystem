using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class FeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List of all Fees
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") == null) return RedirectToAction("Login", "Account");

            // Fetch fees and include Student info (so we can see Names and Room Numbers)
            var fees = await _context.FeeChallans
                .Include(f => f.Student)
                .ThenInclude(s => s.Room)
                .OrderByDescending(f => f.Id) // Show newest bills first
                .ToListAsync();

            return View(fees);
        }

        // POST: Generate Fees for EVERYONE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMonthlyFees()
        {
            // 1. Get the current month (e.g., "December 2025")
            string currentMonth = DateTime.Now.ToString("MMMM yyyy");

            // 2. Get all active students
            var students = await _context.Students.ToListAsync();
            int billsGenerated = 0;

            foreach (var student in students)
            {
                // 3. Check if a bill already exists for this student for this month
                bool billExists = await _context.FeeChallans
                    .AnyAsync(f => f.StudentId == student.Id && f.Month == currentMonth);

                if (!billExists)
                {
                    // 4. Create the new Challan
                    var newFee = new FeeChallan
                    {
                        StudentId = student.Id,
                        Amount = 30000, // Fixed Amount (You can change this)
                        Month = currentMonth,
                        IsPaid = false
                    };

                    _context.FeeChallans.Add(newFee);
                    billsGenerated++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"{billsGenerated} Challans generated for {currentMonth}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Mark a bill as Paid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var challan = await _context.FeeChallans.FindAsync(id);
            if (challan != null)
            {
                challan.IsPaid = true;
                challan.PaidDate = DateTime.Now;
                _context.Update(challan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}