namespace PachecoClinic.Models
{
    public class DoctorAppointmentScheduleViewModel
    {
        public int Id { get; set; }

        public string Subject { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }
    }
}