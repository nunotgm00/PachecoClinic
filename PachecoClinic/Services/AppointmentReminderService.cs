using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;

namespace PachecoClinic.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AppointmentReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using IServiceScope scope = _scopeFactory.CreateScope();

                IAppointmentRepository appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();

                IMailHelper mailHelper = scope.ServiceProvider.GetRequiredService<IMailHelper>();

                DateTime tomorrow = DateTime.Today.AddDays(1);

                List<Appointment> appointments = await appointmentRepository.GetAppointmentsByDateAsync(tomorrow);

                foreach(Appointment appointment in appointments)
                {
                    if (!appointment.ReminderSent)
                    {
                        Response response = mailHelper.SendEmail(
                        appointment.Animal.Client.User.Email!,
                        "Pacheco Clinic - Appointment Reminder",
                        $"<h1>Appointment Reminder</h1>" +
                        $"<p>Hello {appointment.Animal.Client.User.FirstName},</p>" +
                        $"<p>This is a reminder that {appointment.Animal.Name} has an appointment tomorrow.</p>" +
                        $"<p><strong>Date:</strong> {appointment.Date:dd/MM/yyyy}</p>" +
                        $"<p><strong>Time:</strong> {appointment.StartTime:hh\\:mm}</p>" +
                        $"<p><strong>Doctor:</strong> {appointment.Doctor.User.FullName}</p>" +
                        $"<p><strong>Room:</strong> {appointment.Room.Name}</p>");

                        if (response.IsSuccess)
                        {
                            appointment.ReminderSent = true;

                            await appointmentRepository.UpdateAsync(appointment);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
