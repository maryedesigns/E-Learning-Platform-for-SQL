using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Models
{
    public class EditProfileViewModel
    {
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "Max 50 Charaters allowed.")]
        public string? FullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        [MaxLength(50, ErrorMessage = "Max 50 Charaters allowed.")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePic { get; set; } // Accepts file uploads
        public string? ProfilePicturePath { get; set; } // Stores profile pic URL for display

    }
}
