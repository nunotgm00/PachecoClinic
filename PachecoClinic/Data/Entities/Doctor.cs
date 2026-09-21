using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }


        public User User { get; set; }


        public int SpecialtyId { get; set; }


        public Specialty Specialty { get; set; }


        public bool Active { get; set; }

        public ICollection<DoctorSchedule> Schedules { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
