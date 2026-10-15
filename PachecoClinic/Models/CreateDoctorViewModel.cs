using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateDoctorViewModel
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Specialty")]
        public int SpecialtyId { get; set; }

        public bool Active { get; set; } = true;

        public IEnumerable<SelectListItem>? Specialties { get; set; }
    }
}
