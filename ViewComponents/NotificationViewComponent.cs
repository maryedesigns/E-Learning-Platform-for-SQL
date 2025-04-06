using E_LearningProject.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_LearningProject.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var notifications = new List<Notification>();

            if (user != null)
            {
                notifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }

            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);
            return View(notifications);
        }
    }
}
