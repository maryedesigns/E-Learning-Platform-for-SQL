using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [Display(Name = "New PassWord")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "Confirm PassWord")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
