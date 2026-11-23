using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorSchedulesController : Controller
    {
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserHelper _userHelper;

        public DoctorSchedulesController(
            IDoctorScheduleRepository scheduleRepository, 
            IDoctorRepository doctorRepository,
            IUserHelper userHelper)
        {
            _scheduleRepository = scheduleRepository;
            _doctorRepository = doctorRepository;
            _userHelper = userHelper;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_scheduleRepository.GetAllWithDoctors());
        }
        
        [Authorize(Roles = "Admin,EMployee,Doctor")]
        public async Task<IActionResult> Manage(int id)
        {
            Doctor doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Doctor"))
            {
                User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (user == null)
                {
                    return NotFound();
                }

                Doctor? currentDoctor = await _doctorRepository.GetByUserIdAsync(user.Id);

                if(currentDoctor == null || currentDoctor.Id != id)
                {
                    return Forbid();
                }
            }

            List<DoctorSchedule> schedules = await _scheduleRepository.GetByDoctorIdAsync(id);

            DoctorScheduleViewModel model = new DoctorScheduleViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName
            };

            DayOfWeek[] days =
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday,
            };

            foreach (DayOfWeek day in days)
            {
                DoctorSchedule? schedule = schedules.FirstOrDefault(s => s.DayOfWeek == day);

                model.Days.Add(new DoctorScheduleDayViewModel
                {
                    DayOfWeek = day,
                    StartTime = schedule?.StartTime,
                    EndTime = schedule?.EndTime,
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admim,Employee,Doctor")]
        public async Task<IActionResult> Manage(DoctorScheduleViewModel model)
        {
            Doctor? doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(model.DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Doctor"))
            {
                User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (user == null)
                {
                    return NotFound();
                }

                Doctor? currentDoctor = await _doctorRepository.GetByUserIdAsync(user.Id);

                if (currentDoctor == null || currentDoctor.Id != model.DoctorId)
                {
                    return Forbid();
                }
            }

            for (int i = 0; i < model.Days.Count; i++)
            {
                DoctorScheduleDayViewModel day = model.Days[i];

                if(day.StartTime.HasValue != day.EndTime.HasValue)
                {
                    ModelState.AddModelError(
                        $"Days[{i}].StartTime", "Start time and end time must both be filled in");
                }

                if(day.StartTime.HasValue && day.EndTime.HasValue && day.EndTime <= day.StartTime)
                {
                    ModelState.AddModelError(
                        $"Days[{i}].EndTime", "End time must be later than start time.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.DoctorName = doctor.User.FullName;

                return View(model);
            }

            List<DoctorSchedule> schedules = await _scheduleRepository.GetByDoctorIdAsync(model.DoctorId);

            foreach(DoctorScheduleDayViewModel day in model.Days)
            {
                DoctorSchedule? schedule = schedules.FirstOrDefault(s => s.DayOfWeek == day.DayOfWeek);

                if(day.StartTime.HasValue && day.EndTime.HasValue)
                {
                    if(schedule == null)
                    {
                        schedule = new DoctorSchedule
                        {
                            DoctorId = model.DoctorId,
                            DayOfWeek = day.DayOfWeek,
                            StartTime = day.StartTime.Value,
                            EndTime = day.EndTime.Value
                        };

                        await _scheduleRepository.CreateAsync(schedule);
                    }
                    else
                    {
                        schedule.StartTime = day.StartTime.Value;
                        schedule.EndTime = day.EndTime.Value;

                        await _scheduleRepository.UpdateAsync(schedule);
                    }
                }
                else if(schedule != null)
                {
                    await _scheduleRepository.DeleteAsync(schedule);
                }
            }

            TempData["SuccessMessage"] = "Schedule updated successfully.";

            return RedirectToAction(nameof(Manage), new {id = model.DoctorId});
        }

        /*public IActionResult Create()
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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DoctorSchedule? schedule = await _scheduleRepository.GetByIdWithDoctorsAsync(id.Value);

            if(schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            DoctorSchedule? schedule = await _scheduleRepository.GetByIdAsync(id.Value);

            if(schedule == null)
            {
                return NotFound();
            }

            DoctorScheduleViewModel model = new DoctorScheduleViewModel
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Doctors = GetDoctors()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorScheduleViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }

            if(model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time.");
            }

            if (ModelState.IsValid)
            {
                bool overlap = await _scheduleRepository.HasOverlapAsync(
                    model.DoctorId,
                    model.DayOfWeek,
                    model.StartTime,
                    model.EndTime,
                    model.Id);

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

            DoctorSchedule? schedule = await _scheduleRepository.GetByIdAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            schedule.DoctorId = model.DoctorId;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;

            await _scheduleRepository.UpdateAsync(schedule);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            DoctorSchedule? schedule = await _scheduleRepository.GetByIdWithDoctorsAsync(id.Value);

            if(schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DoctorSchedule? schedule = await _scheduleRepository.GetByIdAsync(id);

            if(schedule == null)
            {
                return NotFound();
            }

            await _scheduleRepository.DeleteAsync(schedule);

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
        }*/
    }
}
