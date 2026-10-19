using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        private readonly DataContext _context;

        public DoctorRepository(DataContext context)
            :base(context)
        {
            _context = context;
        }

        public IQueryable<Doctor> GetAllWithUsersAndSpecialties()
        {
            return _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .AsNoTracking();
        }

        public async Task<Doctor?> GetByIdWithUserAndSpecialtyAsync(int id)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id); 
        }

        public async Task<bool> HasAppointmentAsync(int id)
        {
            return await _context.Appointments.AnyAsync(a => id == id);
        }
    }
}
