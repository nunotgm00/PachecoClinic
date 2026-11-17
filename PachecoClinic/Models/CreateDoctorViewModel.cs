using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateDoctorViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;


        [Required]
        [Display(Name = "Specialty")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Specialty")]
        public int? SpecialtyId { get; set; }

        public IEnumerable<SelectListItem>? Specialties { get; set; }
    }
}
