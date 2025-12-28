using Microsoft.AspNetCore.Mvc;
using HostelApp.Data;
using HostelApp.Models;

namespace HostelApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Login()
        {
            return View();
        }

        // POST: Process Login
        [HttpPost]
        public IActionResult Login(string username, string password, string portal)
        {
            // 1. Logic for Warden Portal
            if (portal == "Warden")
            {
                // Auto-create default admin if the table is empty
                if (!_context.Admins.Any())
                {
                    _context.Admins.Add(new Admin
                    {
                        Username = "admin",
                        Password = "admin123",
                        Name = "Head Warden",
                        Role = "Warden"
                    });
                    _context.SaveChanges();
                }

                var admin = _context.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);
                if (admin != null)
                {
                    HttpContext.Session.SetString("Role", "Warden");
                    HttpContext.Session.SetInt32("AdminId", admin.Id);
                    HttpContext.Session.SetString("Name", admin.Name);
                    return RedirectToAction("Index", "Home");
                }
            }
            // 2. Logic for Student Portal
            else
            {
                var student = _context.Students.FirstOrDefault(s => s.StudentRollNo == username && s.CNIC == password);
                if (student != null)
                {
                    HttpContext.Session.SetString("Role", "Student");
                    HttpContext.Session.SetInt32("StudentId", student.Id);
                    HttpContext.Session.SetString("Name", student.Name);
                    return RedirectToAction("Dashboard", "StudentPortal");
                }
            }

            // 3. Login Failed
            ViewBag.Error = $"Invalid credentials for {portal} portal.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ==========================================
        //  PROFILE SECTION
        // ==========================================

        public async Task<IActionResult> Profile()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role == "Student")
            {
                var id = HttpContext.Session.GetInt32("StudentId");
                if (id == null) return RedirectToAction("Login");
                return View(await _context.Students.FindAsync(id));
            }
            else if (role == "Warden")
            {
                ViewBag.Role = "Warden";
                // Get Admin ID from Session
                var adminId = HttpContext.Session.GetInt32("AdminId");
                if (adminId == null) return RedirectToAction("Login");

                // Find the Admin in DB so we can show their real Name
                var admin = await _context.Admins.FindAsync(adminId);

                // Pass the admin object to the view via ViewBag or a separate Model container
                // But since the View expects a 'Student' model usually, we will handle this carefully in the View.
                ViewBag.AdminName = admin.Name;
                ViewBag.AdminUsername = admin.Username;

                return View();
            }

            return RedirectToAction("Login");
        }

        // POST: Update Student Profile
        [HttpPost]
        public async Task<IActionResult> UpdateStudentProfile(int id, string phone, string email, string newPassword)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                student.PhoneNumber = phone;
                student.Email = email;
                if (!string.IsNullOrEmpty(newPassword)) student.CNIC = newPassword;
                _context.Update(student);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Profile updated successfully!";
            }
            return RedirectToAction("Profile");
        }

        // POST: Update Warden (Admin) Password
        [HttpPost]
        public async Task<IActionResult> UpdateWardenPassword(string currentPassword, string newPassword, string newName)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            var admin = await _context.Admins.FindAsync(adminId);

            if (admin != null)
            {
                if (admin.Password == currentPassword)
                {
                    admin.Password = newPassword;
                    // Also allow updating the Name if you want
                    if (!string.IsNullOrEmpty(newName)) admin.Name = newName;

                    _context.Update(admin);
                    await _context.SaveChangesAsync();

                    // Update session name immediately so the top bar updates
                    HttpContext.Session.SetString("Name", admin.Name);

                    TempData["Message"] = "Admin Profile Updated Successfully!";
                }
                else
                {
                    TempData["Error"] = "Current password was incorrect.";
                }
            }
            return RedirectToAction("Profile");
        }
    }
}