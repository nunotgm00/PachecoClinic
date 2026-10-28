using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IAnimalRepository : IGenericRepository<Animal>
    {
        IQueryable<Animal> GetAllWithClients();

        Task<Animal?> GetByIdWithClientAsync(int id);

        Task<bool> HasAppointmentsAsync(int animalId);
    }
}
