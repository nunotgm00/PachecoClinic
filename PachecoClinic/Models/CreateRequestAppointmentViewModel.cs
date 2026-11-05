using PachecoClinic.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateRequestAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        [Display(Name = "Request Type")]
        public AppointmentRequestType? Type { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        [Display(Name = "Requested Date")]
        [DataType(DataType.Date)]
        public DateTime? RequestedDate { get; set; }

        [Display(Name = "Requested Start Time")]
        public TimeSpan? RequestedStartTime { get; set; }
    }
}
