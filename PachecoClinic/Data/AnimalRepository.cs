using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class AnimalRepository : GenericRepository<Animal>, IAnimalRepository
    {
        private readonly DataContext _context;

        public AnimalRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<Animal> GetAllWithClients()
        {
            return _context.Animals
                .Include(a => a.Client)
                .ThenInclude(c => c.User)
                .AsNoTracking();
        }

        public async Task<Animal?> GetByIdWithClientAsync(int id)
        {
            return await _context.Animals
                .Include(a => a.Client)
                .ThenInclude(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> HasAppointmentsAsync(int animalId)
        {
            return await _context.Appointments
                .AnyAsync(a => a.AnimalId == animalId);
        }
    }
}
