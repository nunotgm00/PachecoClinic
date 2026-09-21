using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }

        public Animal Animal { get; set; }

        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; }

        public int RoomId { get; set; }

        public Room Room { get; set; }

        [Required]
        [MaxLength(500)]
        public string Motive { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public AppointmentStatus Status { get; set; }
    }
}
