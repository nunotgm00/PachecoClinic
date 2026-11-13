using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Controllers.API
{
    [ApiController]
    [Route("api/animals/{animalId:int}/interventions")]
    public class InterventionsAPIController : ControllerBase
    {
        private readonly IInterventionRepository _interventionRepository;

        public InterventionsAPIController(IInterventionRepository interventionRepository)
        {
            _interventionRepository = interventionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetInterventions(int animalId)
        {
            List<Intervention> interventions = await _interventionRepository.GetByAnimalIdAsync(animalId);

            var result = interventions.Select(i => new
            {
                i.Id,
                i.AppointmentId,
                Date = i.Appointment.Date,
                StartTime = i.Appointment.StartTime,
                Doctor = i.Appointment.Doctor.User.FullName,
                RoomId = i.Appointment.Room.Name,
                i.Description,
                i.Diagnosis,
                i.Treatment,
                i.Observations
            });

            return Ok(result);
        }
    }
}
