﻿using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Models
{
    public class SignupViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "Max 50 Charaters allowed.")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [MaxLength(50, ErrorMessage = "Max 50 Charaters allowed.")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
