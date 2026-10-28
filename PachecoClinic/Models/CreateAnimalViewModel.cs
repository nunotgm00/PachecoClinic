using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateAnimalViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Species { get; set; } = null!;

        [MaxLength(100)]
        public string? Breed { get; set; }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public decimal? Weight { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(20)]
        public string? Sex { get; set; }

        [Display(Name = "Owner")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an owner.")]
        public int ClientId { get; set; }

        [Required]
        [Display(Name = "Photo")]
        public IFormFile ImageFile { get; set; } = null!;

        public IEnumerable<SelectListItem>? Clients { get; set; }
    }
}
