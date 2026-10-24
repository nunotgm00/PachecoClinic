using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IClientRepository :IGenericRepository<Client>
    {
        IQueryable<Client> GetAllWithUsers();

        Task<Client?> GetByIdWithUserAsync(int id);

        Task<bool> HasAnimalsAsync(int clientId);
    }
}
