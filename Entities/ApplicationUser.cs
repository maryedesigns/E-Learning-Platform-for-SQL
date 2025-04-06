using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string FullName { get; set; }

        public string? ProfilePicturePath { get; set; }

    }
}
