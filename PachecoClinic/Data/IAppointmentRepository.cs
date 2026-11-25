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

        Task<List<Appointment>> GetByAnimalIdWithDetailsAsync(int animalId);

        Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date);

        Task<List<Appointment>> GetFutureAppointmentsByDoctorAndDayAsync(int DoctorId, DayOfWeek dayOfWeek);

        Task<bool> HasAppointmentByRoomAsync(int roomId);

        Task<bool> HasFutureAppointmentsByRoomAsync(int roomId);
    }
}
