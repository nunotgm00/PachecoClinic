using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}