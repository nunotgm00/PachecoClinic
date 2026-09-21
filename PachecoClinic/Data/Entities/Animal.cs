using System.ComponentModel.DataAnnotations;

namespace PachecoClinic.Data.Entities
{
    public class Animal
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Species { get; set; }

        [MaxLength(100)]
        public string? Breed { get; set; }

        public DateTime? BirthDate { get; set; }

        public decimal? Weight { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(20)]
        public string? Sex { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        public int ClientId { get; set; }

        public Client Client { get; set; }
    }
}
