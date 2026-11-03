using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public interface IAppointmentRequestRepository : IGenericRepository<AppointmentRequest>
    {
        IQueryable<AppointmentRequest> GetAllWithAppointmentDetails();

        Task<AppointmentRequest> GetByIdWithAppointmentDetailsAsync(int id);

        Task<bool> HasPendingRequestAsync(int appointmentId);
    }
}
