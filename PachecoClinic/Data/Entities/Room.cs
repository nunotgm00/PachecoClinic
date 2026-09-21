using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Room
    {
        public int Id { get; set; }


        [Display(Name = "Room")]
        [MaxLength(100, ErrorMessage = "The field {0} must have a maximum of {1} characters")]
        [Required(ErrorMessage = "The field {0} is required")]
        public string Name { get; set; }


        [Display(Name = "Type")]
        [MaxLength(100, ErrorMessage = "The field {0} must have a maximum of {1} characters")]
        [Required(ErrorMessage = "The field {0} is required")]
        public string Type { get; set; }


        [Display(Name = "Under Maintenance")]
        public bool UnderMaintenance { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
