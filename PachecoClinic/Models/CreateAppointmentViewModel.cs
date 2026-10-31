using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class CreateAppointmentViewModel
    {
        [Display(Name = "Animal")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an animal.")]
        public int AnimalId { get; set; }

        [Display(Name = "Doctor")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Display(Name = "Room")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a room.")]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public TimeSpan? StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public  TimeSpan? EndTime { get; set; }

        [Required]
        [MaxLength(500)]
        public string Motive { get; set; } = null!;

        public IEnumerable<SelectListItem>? Animals { get; set; }

        public IEnumerable<SelectListItem>? Doctors { get; set; }


        public IEnumerable<SelectListItem>? Rooms { get; set; }



    }
}
