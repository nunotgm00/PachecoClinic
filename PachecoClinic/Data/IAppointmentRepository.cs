using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        IQueryable<Appointment> GetAllWithDetails();

        Task<Appointment?> GetByIdWithDetailsAsync(int id);

        Task<bool> HasConflictAsync(
            int doctorId,
            int roomId,
            int animalId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeId = null);
    }
}
