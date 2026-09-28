using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class AppointmentRequest
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; }

        public AppointmentRequestType Type  { get; set; }

        public AppointmentRequestStatus Status { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        public DateTime? RequestedDate { get; set; }

        public TimeSpan? RequestedStartTime { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
