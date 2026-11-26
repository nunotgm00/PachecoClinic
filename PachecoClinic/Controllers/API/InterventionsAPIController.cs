using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;

namespace PachecoClinic.Controllers.API
{
    [ApiController]
    [Route("api/animals/{animalId:int}/interventions")]
    [Authorize(Roles = "Admin,Employee,Doctor,CLient")]
    public class InterventionsAPIController : ControllerBase
    {
        private readonly IInterventionRepository _interventionRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserHelper _userHelper;

        public InterventionsAPIController(IInterventionRepository interventionRepository,
            IAnimalRepository animalRepository,
            IClientRepository clientRepository,
            IUserHelper userHelper)
        {
            _interventionRepository = interventionRepository;
            _animalRepository = animalRepository;
            _clientRepository = clientRepository;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetInterventions(int animalId)
        {
            Animal? animal = await _animalRepository.GetByIdAsync(animalId);

            if(animal == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if(user == null)
                {
                    return NotFound();
                }

                Client? client = await _clientRepository.GetByUserIdAsync(user.Id);

                if(client == null)
                {
                    return NotFound();
                }

                if(client.Id != animal.ClientId)
                {
                    return Forbid();
                }
            }

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
