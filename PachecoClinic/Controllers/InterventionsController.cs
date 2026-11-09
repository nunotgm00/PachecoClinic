using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize]
    public class InterventionsController : Controller
    {
        private readonly IInterventionRepository _interventionRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserHelper _userHelper;

        public InterventionsController(
            IInterventionRepository interventionRepository,
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IUserHelper userHelper)
        {
            _interventionRepository = interventionRepository;
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _userHelper = userHelper;   
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

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create(int appointmentId)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            if(appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if(user == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByUserIdAsync(user.Id);

            if(doctor == null)
            {
                return NotFound();
            }

            if(appointment.DoctorId != doctor.Id)
            {
                return Forbid();
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
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInterventionViewModel model)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdAsync(model.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByUserIdAsync(user.Id);

            if (doctor == null)
            {
                return NotFound();
            }

            if (appointment.DoctorId != doctor.Id)
            {
                return Forbid();
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
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

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Intervention? intervention = await _interventionRepository.GetByIdAsync(id.Value);

            if(intervention == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdAsync(intervention.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByUserIdAsync(user.Id);

            if (doctor == null)
            {
                return NotFound();
            }

            if (appointment.DoctorId != doctor.Id)
            {
                return Forbid();
            }


            EditInterventionViewModel model = new EditInterventionViewModel
            {
                Id = intervention.Id,
                AppointmentId = intervention.AppointmentId,
                Description = intervention.Description,
                Diagnosis = intervention.Diagnosis,
                Treatment = intervention.Treatment,
                Observations = intervention.Observations
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  EditInterventionViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }

            Intervention? intervention = await _interventionRepository.GetByIdAsync(id);

            if(intervention == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdAsync(intervention.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByUserIdAsync(user.Id);

            if (doctor == null)
            {
                return NotFound();
            }

            if (appointment.DoctorId != doctor.Id)
            {
                return Forbid();
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            intervention.Description = model.Description;
            intervention.Diagnosis = model.Diagnosis;
            intervention.Treatment = model.Treatment;
            intervention.Observations = model.Observations;

            await _interventionRepository.UpdateAsync(intervention);

            return RedirectToAction(nameof(Details), new { id = intervention.Id });
        }
    }
}
