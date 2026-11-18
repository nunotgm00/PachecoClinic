namespace PachecoClinic.Models
{
    public class AnimalListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Species { get; set; } = null!;

        public string? Breed { get; set; }

        public string Owner { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        public decimal? Weight { get; set; }
    }
}