using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Models
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }

        [ValidateNever]
        public string DoctorName { get; set; } = null!;

        public List<DoctorScheduleDayViewModel> Days { get; set; } = new();
    }
}
