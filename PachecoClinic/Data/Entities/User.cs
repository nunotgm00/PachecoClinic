using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(50, ErrorMessage = "The field [0] only can contain {1} characters length.")]
        [Required]
        public string FirstName { get; set; }


        [MaxLength(50, ErrorMessage = "The field [0] only can contain {1} characters length.")]
        [Required]
        public string LastName { get; set; }


        public string? ImageUrl { get; set; }


        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
