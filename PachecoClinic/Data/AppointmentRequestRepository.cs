using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class AppointmentRequestRepository : GenericRepository<AppointmentRequest>, IAppointmentRequestRepository
    {
        private readonly DataContext _context;
        public AppointmentRequestRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<AppointmentRequest> GetAllWithAppointmentDetails()
        {
            return _context.AppointmentRequests
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Animal)
                        .ThenInclude(a => a.Client)
                            .ThenInclude(c => c.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .AsNoTracking();
        }

        public async Task<AppointmentRequest?> GetByIdWithAppointmentDetailsAsync(int id)
        {
            return await _context.AppointmentRequests
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Animal)
                        .ThenInclude(a => a.Client)
                            .ThenInclude(c => c.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> HasPendingRequestAsync(int appointmentId)
        {
            return await _context.AppointmentRequests
                .AnyAsync(r => r.AppointmentId == appointmentId && r.Status == AppointmentRequestStatus.Pending); 
        }
    }
}
