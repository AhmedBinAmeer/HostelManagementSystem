using Microsoft.EntityFrameworkCore;
using HostelApp.Models;

namespace HostelApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing Tables
        public DbSet<Student> Students { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<FeeChallan> FeeChallans { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // Added this line to fix your error
       
    }
}