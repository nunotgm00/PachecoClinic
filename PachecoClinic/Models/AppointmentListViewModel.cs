namespace PachecoClinic.Models
{
    public class AppointmentListViewModel
    {
        public int Id { get; set; }

        public string Animal { get; set; } = string.Empty;

        public string Owner { get; set; } = string.Empty;

        public string Doctor { get; set; } = string.Empty;

        public string Room { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Time { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool CanManage { get; set; }
    }
}