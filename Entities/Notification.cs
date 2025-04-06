using Microsoft.AspNetCore.Identity;

namespace E_LearningProject.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key to Identity User
        public ApplicationUser User { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
