using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; }

        public List<DoctorScheduleDayViewModel> Days { get; set; } = new();
    }
}
