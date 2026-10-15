using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        IQueryable<Doctor> GetAllWithUsersAndSpecialties();

        Task<Doctor?> GetByIdWithUserAndSpecialtyAsync(int id);
    }
}
