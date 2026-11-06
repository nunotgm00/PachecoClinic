using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IInterventionRepository : IGenericRepository<Intervention>
    {
        IQueryable<Intervention> GetAllWithDetails();

        Task<Intervention?> GetByIdWithDetailsAsync(int id);

        Task<bool> HasInterventionsAsync(int appointmentId);
    }
}
