using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class ProfileModelView
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
