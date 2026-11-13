using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class InterventionRepository : GenericRepository<Intervention>, IInterventionRepository
    {
        private readonly DataContext _context;

        public InterventionRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<Intervention> GetAllWithDetails()
        {
            return _context.Interventions
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Animal)
                        .ThenInclude(a => a.Client)
                            .ThenInclude(c => c.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Room)
                .AsNoTracking();
        }

        public async Task<List<Intervention>> GetByAnimalIdAsync(int animalId)
        {
            return await _context.Interventions
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Room)
                .Where(i => i.Appointment.AnimalId == animalId)
                .OrderByDescending(i => i.Appointment.Date)
                .ThenByDescending(i => i.Appointment.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Intervention?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Interventions
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Animal)
                        .ThenInclude(a => a.Client)
                            .ThenInclude(c => c.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Room)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> HasInterventionsAsync(int appointmentId)
        {
            return await _context.Interventions
                .AnyAsync(i => i.AppointmentId == appointmentId);
        }
    }
}
