using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly DataContext _context;

        public AppointmentRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Appointment> GetAllWithDetails()
        {
            return _context.Appointments
                .Include(a => a.Animal)
                    .ThenInclude(a => a.Client)
                        .ThenInclude(c => c.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Room)
                .AsNoTracking();
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Animal)
                    .ThenInclude(a => a.Client)
                        .ThenInclude(c => c.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Room)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> HasConflictAsync(
            int doctorId,
            int roomId,
            int animalId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime, 
            int? excludeId = null)
        {
            IQueryable<Appointment> appointments = _context.Appointments
                .Where(a => a.Date.Date == date.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                startTime < a.EndTime &&
                endTime < a.StartTime &&
                (
                    a.DoctorId == doctorId ||
                    a.RoomId == roomId ||
                    a.AnimalId == animalId
                ));

            if(excludeId != null)
            {
                appointments = appointments.Where(a => a.Id != excludeId.Value);
            }

            return await appointments.AnyAsync();
        }
    }
}
