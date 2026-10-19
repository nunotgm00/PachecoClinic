using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IDoctorScheduleRepository : IGenericRepository<DoctorSchedule>
    {
        IQueryable<DoctorSchedule> GetAllWithDoctors();

        Task<DoctorSchedule?> GetByIdWithDoctorsAsync(int id);

        Task<bool> HasOverlapAsync(
            int doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeId = null);
    }
}
