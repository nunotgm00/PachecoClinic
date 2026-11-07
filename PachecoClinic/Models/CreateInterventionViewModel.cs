using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateInterventionViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [MaxLength(500)]
        public string? Diagnosis {  get; set; }

        [MaxLength(500)]
        public string? Treatment { get; set; }

        [MaxLength(500)]
        public string? Observations { get; set; }


    }
}
