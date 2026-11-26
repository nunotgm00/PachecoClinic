using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IUserHelper _userHelper;
        private readonly IClientRepository _clientRepository;
        private readonly IMailHelper _mailHelper;

        public AppointmentsController(
            IAppointmentRepository appointmentRepository,
            IAnimalRepository animalRepository,
            IDoctorRepository doctorRepository,
            IGenericRepository<Room> roomRepository,
            IDoctorScheduleRepository scheduleRepository,
            IUserHelper userHelper,
            IClientRepository clientRepository,
            IMailHelper mailHelper)
        {
            _appointmentRepository = appointmentRepository;
            _animalRepository = animalRepository;
            _doctorRepository = doctorRepository;
            _roomRepository = roomRepository;
            _scheduleRepository = scheduleRepository;
            _userHelper = userHelper;
            _clientRepository = clientRepository;
            _mailHelper = mailHelper;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            string? currentUserEmail = User.Identity?.Name;
            bool isDoctor = User.IsInRole("Doctor");

            List<AppointmentListViewModel> model = _appointmentRepository
                .GetAllWithDetails()
                .Select(a => new AppointmentListViewModel
                {
                    Id = a.Id,
                    Animal = a.Animal.Name,
                    Owner = a.Animal.Client.User.FullName,
                    Doctor = a.Doctor.User.FullName,
                    Room = $"{a.Room.Name} - {a.Room.Type}",
                    Date = a.Date,
                    Time = $"{a.StartTime:hh\\:mm} - {a.EndTime:hh\\:mm}",
                    Status = a.Status.ToString(),
                    CanManage =
                        a.Status == AppointmentStatus.Scheduled &&
                        a.Date.Date + a.StartTime > DateTime.Now &&
                        (!isDoctor || a.Doctor.User.Email == currentUserEmail)
                })
                .ToList();

            return View(model);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Create()
        {
            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor") && currentDoctor == null)
            {
                return NotFound();
            }

            IEnumerable<SelectListItem> doctors = GetDoctors();

            if (currentDoctor != null)
            {
                doctors = doctors.Where(d => d.Value == currentDoctor.Id.ToString());
            }

            CreateAppointmentViewModel model = new CreateAppointmentViewModel
            {
                Animals = GetAnimals(),
                Doctors = doctors,
                Rooms = GetRooms()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor") && currentDoctor == null)
            {
                return NotFound();
            }

            Animal? animal = await _animalRepository.GetByIdAsync(model.AnimalId);
            Doctor? doctor = await _doctorRepository.GetByIdAsync(model.DoctorId);
            Room? room = await _roomRepository.GetByIdAsync(model.RoomId);

            if (animal == null)
            {
                ModelState.AddModelError("AnimalId", "The selected animal does not exist.");
            }

            if (doctor == null || !doctor.Active)
            {
                ModelState.AddModelError("DoctorId", "The selected doctor is not available.");
            }

            if (currentDoctor != null && model.DoctorId != currentDoctor.Id)
            {
                return Forbid();
            }

            if (room == null || room.UnderMaintenance)
            {
                ModelState.AddModelError("RoomId", "The selected room is not available.");
            }

            if (model.Date.HasValue && model.Date.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Date", "The appointment date cannot be in the past");
            }

            if(model.StartTime.HasValue && model.EndTime.HasValue && model.EndTime.Value <= model.StartTime.Value)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time");
            }

            if (model.Date.HasValue && model.StartTime.HasValue)
            {
                DateTime appointmentDateTime = model.Date.Value.Date + model.StartTime.Value;

                if (appointmentDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("StartTime", "The appointment date and time must be in the future.");
                }
            }

            if (ModelState.IsValid)
            {
                bool doctorAvailable = await _scheduleRepository.IsDoctorAvailableAsync(
                    model.DoctorId,
                    model.Date!.Value.DayOfWeek,
                    model.StartTime!.Value,
                    model.EndTime!.Value);

                if (!doctorAvailable)
                {
                    ModelState.AddModelError(string.Empty,
                        "The doctor is not available during the selected date and time");
                }
            }

            if (ModelState.IsValid)
            {
                bool conflict = await _appointmentRepository.HasConflictAsync(
                        model.DoctorId,
                        model.RoomId,
                        model.AnimalId,
                        model.Date!.Value,
                        model.StartTime!.Value,
                        model.EndTime!.Value);

                if (conflict)
                {
                    ModelState.AddModelError(string.Empty, "There is already an appointment conflicting with the selected doctor, room or animal.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Animals = GetAnimals();
                model.Doctors = currentDoctor != null
                    ? GetDoctors().Where(d => d.Value == currentDoctor.Id.ToString())
                    : GetDoctors();
                model.Rooms = GetRooms();

                return View(model);
            }

            Appointment appointment = new Appointment
            {
                AnimalId = model.AnimalId,
                DoctorId = model.DoctorId,
                RoomId = model.RoomId,
                Date = model.Date!.Value,
                StartTime = model.StartTime!.Value,
                EndTime = model.EndTime!.Value,
                Motive = model.Motive,
                Status = AppointmentStatus.Scheduled
            };

            await _appointmentRepository.CreateAsync(appointment);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee,Doctor,Client")]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id.Value);

            if(appointment == null)
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

                if(appointment.Animal.ClientId != client.Id)
                {
                    return Forbid();
                }
            }

            return View(appointment);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdAsync(id.Value);

            if(appointment == null)
            {
                return NotFound();
            }

            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor"))
            {
                if (currentDoctor == null)
                {
                    return NotFound();
                }

                if (appointment.DoctorId != currentDoctor.Id)
                {
                    return Forbid();
                }
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest();
            }

            DateTime appointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if (appointmentDateTime <= DateTime.Now)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            IEnumerable<SelectListItem> doctors = GetDoctors();

            if (currentDoctor != null)
            {
                doctors = doctors.Where(d => d.Value == currentDoctor.Id.ToString());
            }

            EditAppointmentViewModel model = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                AnimalId = appointment.AnimalId,
                DoctorId = appointment.DoctorId,
                RoomId = appointment.RoomId,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Motive = appointment.Motive,
                Animals = GetAnimals(),
                Doctors = doctors,
                Rooms = GetRooms()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAppointmentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor"))
            {
                if (currentDoctor == null)
                {
                    return NotFound();
                }

                if (appointment.DoctorId != currentDoctor.Id || model.DoctorId != currentDoctor.Id)
                {
                    return Forbid();
                }
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest("Only scheduled appointments can be edited.");
            }

            DateTime currentAppointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if (currentAppointmentDateTime <= DateTime.Now)
            {
                return BadRequest("Past appointments cannot be edited.");
            }

            Animal? animal = await _animalRepository.GetByIdAsync(model.AnimalId);

            if (animal == null)
            {
                ModelState.AddModelError("AnimalId", "The selected animal does not exist.");
            }

            Doctor? doctor = await _doctorRepository.GetByIdAsync(model.DoctorId);

            if (doctor == null)
            {
                ModelState.AddModelError("DoctorId", "The selected doctor does not exist.");
            }
            else if (!doctor.Active)
            {
                ModelState.AddModelError("DoctorId", "The selected doctor is not active.");
            }

            Room? room = await _roomRepository.GetByIdAsync(model.RoomId);

            if (room == null)
            {
                ModelState.AddModelError("RoomId", "The selected room does not exist.");
            }
            else if (room.UnderMaintenance)
            {
                ModelState.AddModelError("RoomId", "The selected room is under maintenance.");
            }

            if (model.StartTime.HasValue && model.EndTime.HasValue &&
                model.EndTime.Value <= model.StartTime.Value)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time.");
            }

            if (model.Date.HasValue && model.StartTime.HasValue)
            {
                DateTime newAppointmentDateTime = model.Date.Value.Date + model.StartTime.Value;

                if (newAppointmentDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("StartTime", "The appointment date and time must be in the future.");
                }
            }

            if (ModelState.IsValid)
            {
                bool doctorAvailable = await _scheduleRepository.IsDoctorAvailableAsync(
                    model.DoctorId,
                    model.Date!.Value.DayOfWeek,
                    model.StartTime!.Value,
                    model.EndTime!.Value);

                if (!doctorAvailable)
                {
                    ModelState.AddModelError(string.Empty,
                        "The doctor is not available during the selected date and time.");
                }
            }

            if (ModelState.IsValid)
            {
                bool conflict = await _appointmentRepository.HasConflictAsync(
                    model.DoctorId,
                    model.RoomId,
                    model.AnimalId,
                    model.Date!.Value,
                    model.StartTime!.Value,
                    model.EndTime!.Value,
                    model.Id);

                if (conflict)
                {
                    ModelState.AddModelError(string.Empty,
                        "There is already an appointment conflicting with the selected doctor, room or animal.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Animals = GetAnimals();
                model.Doctors = currentDoctor != null
                    ? GetDoctors().Where(d => d.Value == currentDoctor.Id.ToString())
                    : GetDoctors();
                model.Rooms = GetRooms();

                return View(model);
            }

            bool scheduleChanged =
                appointment.Date.Date != model.Date!.Value.Date ||
                appointment.StartTime != model.StartTime!.Value ||
                appointment.EndTime != model.EndTime!.Value ||
                appointment.DoctorId != model.DoctorId;

            appointment.AnimalId = model.AnimalId;
            appointment.DoctorId = model.DoctorId;
            appointment.RoomId = model.RoomId;
            appointment.Date = model.Date!.Value;
            appointment.StartTime = model.StartTime!.Value;
            appointment.EndTime = model.EndTime!.Value;
            appointment.Motive = model.Motive;

            if (scheduleChanged)
            {
                appointment.ReminderSent = false;
            }

            await _appointmentRepository.UpdateAsync(appointment);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id.Value);

            if (appointment == null)
            {
                return NotFound();
            }

            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor"))
            {
                if (currentDoctor == null)
                {
                    return NotFound();
                }

                if (appointment.DoctorId != currentDoctor.Id)
                {
                    return Forbid();
                }
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                return RedirectToAction(nameof(Details), new { id = appointment.Id });
            }

            DateTime appointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if (appointmentDateTime <= DateTime.Now)
            {
                return RedirectToAction(nameof(Details), new { id = appointment.Id });
            }

            return View(appointment);
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdAsync(id);

            if(appointment == null)
            {
                return NotFound();
            }

            Doctor? currentDoctor = await GetCurrentDoctorAsync();

            if (User.IsInRole("Doctor"))
            {
                if (currentDoctor == null)
                {
                    return NotFound();
                }

                if (appointment.DoctorId != currentDoctor.Id)
                {
                    return Forbid();
                }
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                ModelState.AddModelError(string.Empty, "Only scheduled appointments can be cancelled");

                Appointment? appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(id);

                return View("Cancel", appointmentWithDetails);
            }

            DateTime appointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if(appointmentDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Past appointments cannot be cancelled");

                Appointment? appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(id);

                return View("Cancel", appointmentWithDetails);
            }

            appointment.Status = AppointmentStatus.Cancelled;

            await _appointmentRepository.UpdateAsync(appointment);

            Appointment? appointmentWithDetailsForEmail = await _appointmentRepository.GetByIdWithDetailsAsync(id);

            if(appointmentWithDetailsForEmail != null)
            {
                Response response = _mailHelper.SendEmail(
                    appointmentWithDetailsForEmail.Animal.Client.User.Email!,
                    "Pacheco Clinic - Appointment Cancelled",
                    $"<h1>Appointment Cancelled</h1>" +
                    $"<p>Hello {appointmentWithDetailsForEmail.Animal.Client.User.FirstName},</p>" +
                    $"<p>Your appointment for <strong>{appointmentWithDetailsForEmail.Animal.Name}</strong> has been cancelled.</p>" +
                    $"<p><strong>Date:</strong> {appointmentWithDetailsForEmail.Date:dd/MM/yyyy}</p>" +
                    $"<p><strong>Time:</strong> {appointmentWithDetailsForEmail.StartTime:hh\\:mm}</p>" +
                    $"<p><strong>Doctor:</strong> {appointmentWithDetailsForEmail.Doctor.User.FullName}</p>" +
                    $"<p>Please contact Pacheco Clinic if you wish to schedule another appointment.</p>");

                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] =
                        "Appointment cancelled successfully. The client was notified by email.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Appointment cancelled succesfully, but the email could not be sent to client";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyAppointments()
        {
            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByUserIdAsync(user.Id);

            if (client == null)
            {
                return NotFound();
            }

            List<MyAppointmentListViewModel> model = _appointmentRepository
                .GetAllWithDetails()
                .Where(a => a.Animal.ClientId == client.Id)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .Select(a => new MyAppointmentListViewModel
                {
                    Id = a.Id,
                    Animal = a.Animal.Name,
                    Doctor = a.Doctor.User.FullName,
                    Room = $"{a.Room.Name} - {a.Room.Type}",
                    Date = a.Date,
                    Time = $"{a.StartTime:hh\\:mm} - {a.EndTime:hh\\:mm}",
                    Status = a.Status.ToString(),

                    HasPendingRequest = a.Requests.Any(r =>
                        r.Status == AppointmentRequestStatus.Pending),

                    CanRequestChange =
                        a.Status == AppointmentStatus.Scheduled &&
                        a.Date.Date + a.StartTime > DateTime.Now &&
                        !a.Requests.Any(r => r.Status == AppointmentRequestStatus.Pending)
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);

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

            if(appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest();
            }

            DateTime appointmentStart = appointment.Date.Date + appointment.StartTime;

            if(appointmentStart > DateTime.Now)
            {
                TempData["ErrorMessage"] = "A future appointment cannot be completed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            appointment.Status = AppointmentStatus.Completed;

            await _appointmentRepository.UpdateAsync(appointment);

            return RedirectToAction(nameof(Details), new { id });
        }

        private IEnumerable<SelectListItem> GetAnimals()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach (Animal animal in _animalRepository.GetAllWithClients())
            {
                items.Add(new SelectListItem
                {
                    Value = animal.Id.ToString(),
                    Text = $"{animal.Name} - {animal.Client.User.FullName}"
                });
            }

            return items;
        }

        private IEnumerable<SelectListItem> GetDoctors()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach (Doctor doctor in _doctorRepository.GetAllWithUsersAndSpecialties())
            {
                if (doctor.Active)
                {
                    items.Add(new SelectListItem
                    {
                        Value = doctor.Id.ToString(),
                        Text = $"{doctor.User.FullName} - {doctor.Specialty.Name}"
                    });
                }
            }

            return items;
        }

        private IEnumerable<SelectListItem> GetRooms()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach (Room room in _roomRepository.GetAll())
            {
                if (!room.UnderMaintenance)
                {
                    items.Add(new SelectListItem
                    {
                        Value = room.Id.ToString(),
                        Text = $"{room.Name} - {room.Type}"
                    });
                }
            }

            return items;
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            if (!User.IsInRole("Doctor"))
            {
                return null;
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return null;
            }

            return await _doctorRepository.GetByUserIdAsync(user.Id);
        }
    }
}
