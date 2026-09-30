using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Intervention : IEntity
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string? Diagnosis { get; set; }

        [MaxLength(500)]
        public string? Treatment { get; set; }

        [MaxLength(1000)]
        public string? Observations { get; set; }
    }
}
