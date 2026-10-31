using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class DoctorScheduleRepository : GenericRepository<DoctorSchedule>, IDoctorScheduleRepository
    {
        private readonly DataContext _context;

        public DoctorScheduleRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<DoctorSchedule> GetAllWithDoctors()
        {
            return _context.DoctorSchedules
                .Include(s => s.Doctor)
                .ThenInclude(d => d.User)
                .AsNoTracking();
        }

        public async Task<DoctorSchedule?> GetByIdWithDoctorsAsync(int id)
        {
            return await _context.DoctorSchedules
                .Include(s => s.Doctor)
                .ThenInclude(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> HasOverlapAsync(int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            IQueryable<DoctorSchedule> schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId &&
                s.DayOfWeek == dayOfWeek &&
                s.StartTime < s.EndTime &&
                endTime > startTime);

            if(excludeId != null)
            {
                schedules = schedules.Where(s => s.Id != excludeId.Value);
            }

            return await schedules.AnyAsync();
        }

        public async Task<bool> IsDoctorAvailableAsync(
            int doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            return await _context.DoctorSchedules
                .AnyAsync(s => s.DoctorId == doctorId
                    && s.DayOfWeek == dayOfWeek
                    && startTime >= s.StartTime
                    && endTime <= s.EndTime);
        }
    }
}
