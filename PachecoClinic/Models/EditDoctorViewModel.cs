using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class EditDoctorViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Specialty")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a specialty.")]
        public int SpecialtyId { get; set; }

        public bool Active { get; set; }

        public IEnumerable<SelectListItem>? Specialties { get; set; }
    }
}
