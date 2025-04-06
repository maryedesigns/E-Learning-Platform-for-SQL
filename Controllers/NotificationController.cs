using E_LearningProject.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_LearningProject.Controllers
{
    public class NotificationController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Handle the case where the user is not authenticated
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not authenticated
            }

            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.Notifications = notifications ?? new List<Notification>(); // Handle null safely
            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);

            return View(notifications);
        }

        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");  // Redirect back to notifications page or wherever appropriate
        }

    }
}
