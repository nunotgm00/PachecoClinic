using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Client
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }


        [MaxLength(20)]
        public string? Nif { get; set; }


        [Required]
        public string UserId { get; set; }


        public User User { get; set; }


        public ICollection<Animal> Animals { get; set; }
    }
}
