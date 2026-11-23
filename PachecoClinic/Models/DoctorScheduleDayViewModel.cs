namespace PachecoClinic.Models
{
    public class DoctorScheduleDayViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }
    }
}
