namespace PachecoClinic.Models
{
    public class ClientListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Nif { get; set; }

        public string? Address { get; set; }
    }
}