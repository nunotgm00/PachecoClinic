using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Specialty
    {
        public int Id { get; set; }


        [Display(Name = "Specialty")]
        [MaxLength(100, ErrorMessage = "The field {0} must have a maximum of {1} characters")]
        [Required(ErrorMessage = "The field {0} is required")]
        public string Name { get; set; }
    }
}
