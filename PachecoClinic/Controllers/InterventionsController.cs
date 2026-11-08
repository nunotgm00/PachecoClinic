using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize]
    public class InterventionsController : Controller
    {
        private readonly IInterventionRepository _interventionRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public InterventionsController(
            IInterventionRepository interventionRepository,
            IAppointmentRepository appointmentRepository)
        {
            _interventionRepository = interventionRepository;
            _appointmentRepository = appointmentRepository;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            IQueryable<Intervention> interventions = _interventionRepository.GetAllWithDetails();

            return View(interventions);
        }

        [Authorize(Roles = ("Admin,Employee,Doctor"))]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Intervention? intervention = await _interventionRepository.GetByIdWithDetailsAsync(id.Value);

            if(intervention == null)
            {
                return NotFound();
            }

            return View(intervention);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Create(int appointmentId)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);

            if(appointment == null)
            {
                return NotFound();
            }

            if(appointment.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest("An intervention cannot be created for a cancelled appointment");
            }

            if(await _interventionRepository.HasInterventionsAsync(appointmentId))
            {
                return BadRequest("This appointment already has an intervention.");
            }

            CreateInterventionViewModel model = new CreateInterventionViewModel
            {
                AppointmentId = appointmentId
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInterventionViewModel model)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdAsync(model.AppointmentId);

            if(appointment == null)
            {
                return NotFound();
            }

            if(appointment.Status == AppointmentStatus.Cancelled)
            {
                ModelState.AddModelError(string.Empty, "An intervention cannot be created for a cancelled appointment.");
            }

            if(await _interventionRepository.HasInterventionsAsync(model.AppointmentId))
            {
                ModelState.AddModelError(string.Empty, "This appointment already has an intervention");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Intervention intervention = new Intervention
            {
                Appointment = appointment,
                Description = model.Description,
                Diagnosis = model.Diagnosis,
                Treatment = model.Treatment,
                Observations = model.Observations
            };

            await _interventionRepository.CreateAsync(intervention);

            return RedirectToAction(nameof(Details), new {id = intervention.Id});
        }
    }
}
