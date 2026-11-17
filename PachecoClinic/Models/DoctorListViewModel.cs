namespace PachecoClinic.Models
{
    public class DoctorListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string Specialty { get; set; } = null!;

        public bool Active { get; set; }

        public string? ImageUrl { get; set; }
    }
}