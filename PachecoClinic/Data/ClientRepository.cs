using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        private readonly DataContext _context;

        public ClientRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Client> GetAllWithUsers()
        {
            return _context.Clients
                .Include(c => c.User)
                .AsNoTracking();
        }

        public async Task<Client?> GetByIdWithUserAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> HasAnimalsAsync(int clientId)
        {
            return await _context.Animals
                .AnyAsync(a => a.ClientId == clientId);
        }
    }
}
