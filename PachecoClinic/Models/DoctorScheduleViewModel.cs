using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class DoctorScheduleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Doctor")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Display(Name = "Day")]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        public IEnumerable<SelectListItem>? Doctors { get; set; }
    }
}
