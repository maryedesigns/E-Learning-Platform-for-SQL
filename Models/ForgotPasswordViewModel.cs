using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
