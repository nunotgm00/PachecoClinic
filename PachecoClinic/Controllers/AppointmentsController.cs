using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin,Employee,Doctor")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IDoctorScheduleRepository _scheduleRepository;

        public AppointmentsController(
            IAppointmentRepository appointmentRepository,
            IAnimalRepository animalRepository,
            IDoctorRepository doctorRepository,
            IGenericRepository<Room> roomRepository,
            IDoctorScheduleRepository scheduleRepository)
        {
            _appointmentRepository = appointmentRepository;
            _animalRepository = animalRepository;
            _doctorRepository = doctorRepository;
            _roomRepository = roomRepository;
            _scheduleRepository = scheduleRepository;
        }

        public IActionResult Index()
        {
            return View(_appointmentRepository.GetAllWithDetails());
        }

        public IActionResult Create()
        {
            CreateAppointmentViewModel model = new CreateAppointmentViewModel()
            {
                Animals = GetAnimals(),
                Doctors = GetDoctors(),
                Rooms = GetRooms(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            if(model.Date.HasValue && model.Date.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Date", "The appointment date cannot be in the past");
            }

            if(model.StartTime.HasValue && model.EndTime.HasValue && model.EndTime.Value <= model.StartTime.Value)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time");
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
                bool conflict =
                    await _appointmentRepository.HasConflictAsync(
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
                model.Doctors = GetDoctors();
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

            return View(appointment);
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
    }
}
