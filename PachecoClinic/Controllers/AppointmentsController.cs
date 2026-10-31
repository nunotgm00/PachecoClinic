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
            return View(_appointmentRepository.GetAllWithDetails);
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

        pu

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
