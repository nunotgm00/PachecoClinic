using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class ClinicClosureViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Closure Date")]
        public DateTime? Date { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = null!;
    }
}
