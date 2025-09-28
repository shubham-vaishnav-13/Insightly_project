using System.ComponentModel.DataAnnotations;

namespace Insightly_project.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")] public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")] public string Password { get; set; }

        [Display(Name = "Remember Me")] public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
