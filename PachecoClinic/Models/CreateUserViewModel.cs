using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        [Display(Name = "Specialty")]
        public int? SpecialtyId { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }

        public IEnumerable<SelectListItem>? Specialties { get; set; }
    }
}
