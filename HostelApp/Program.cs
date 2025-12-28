using Microsoft.EntityFrameworkCore;
using HostelApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --- DATABASE CONFIGURATION ---
// Connects the HMS Project to MS SQL Server using the connection string in appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- ROLE-BASED SESSION AUTHENTICATION ---
// Required to keep Warden or Student logged in across different pages
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// --- MIDDLEWARE PIPELINE ---
// Enables Session support; must be placed after UseRouting and before MapControllerRoute
app.UseSession();

app.MapControllerRoute(
    name: "default",
    // Redirects the user to the Login page immediately upon starting the application
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();