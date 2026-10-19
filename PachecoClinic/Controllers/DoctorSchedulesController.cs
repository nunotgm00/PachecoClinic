using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorSchedulesController : Controller
    {
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IDoctorRepository _doctorRepository;

        public DoctorSchedulesController(
            IDoctorScheduleRepository scheduleRepository, 
            IDoctorRepository doctorRepository)
        {
            _scheduleRepository = scheduleRepository;
            _doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View(_scheduleRepository.GetAllWithDoctors());
        }

        public IActionResult Create()
        {
            DoctorScheduleViewModel model = new DoctorScheduleViewModel
            {
                Doctors = GetDoctors()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorScheduleViewModel model)
        {
            if(model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime",
                    "End time must be later than start time.");
            }

            if (ModelState.IsValid)
            {
                bool overlap = await _scheduleRepository.HasOverlapAsync(
                    model.DoctorId,
                    model.DayOfWeek,
                    model.StartTime,
                    model.EndTime);

                if (overlap)
                {
                    ModelState.AddModelError(string.Empty,
                        "This schedule overlaps with another schedule for this doctor.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Doctors = GetDoctors();

                return View(model);
            }

            DoctorSchedule shcedule = new DoctorSchedule
            {
                DoctorId = model.DoctorId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
            };

            await _scheduleRepository.CreateAsync(shcedule);

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetDoctors()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach(Doctor doctor in _doctorRepository.GetAllWithUsersAndSpecialties())
            {
                if (doctor.Active)
                {
                    items.Add(new SelectListItem
                    {
                        Value = doctor.Id.ToString(),
                        Text = doctor.User.FullName
                    });
                }
            }

            return items;
        }
    }
}
