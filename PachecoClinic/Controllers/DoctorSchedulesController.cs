using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin,Employee,Doctor")]
    public class DoctorSchedulesController : Controller
    {
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserHelper _userHelper;
        private readonly IAppointmentRepository _appointmentRepository;

        public DoctorSchedulesController(
            IDoctorScheduleRepository scheduleRepository, 
            IDoctorRepository doctorRepository,
            IUserHelper userHelper,
            IAppointmentRepository appointmentRepository)
        {
            _scheduleRepository = scheduleRepository;
            _doctorRepository = doctorRepository;
            _userHelper = userHelper;
            _appointmentRepository = appointmentRepository;
        }
        
        public async Task<IActionResult> Manage(int id)
        {
            Doctor? doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id);

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

            foreach(DoctorScheduleDayViewModel day in model.Days)
            {
                List<Appointment> appointments =
                    await _appointmentRepository.GetFutureAppointmentsByDoctorAndDayAsync(model.DoctorId, day.DayOfWeek);

                if(!day.StartTime.HasValue && !day.EndTime.HasValue)
                {
                    if (appointments.Any())
                    {
                        ModelState.AddModelError(string.Empty, $"Cannot set {day.DayOfWeek} as a day off because there are future appointments scheduled.");
                    }
                }
                else
                {
                    bool invalidAppointment = appointments.Any(a => a.StartTime < day.StartTime!.Value ||
                        a.EndTime > day.EndTime!.Value);

                    if (invalidAppointment)
                    {
                        ModelState.AddModelError(string.Empty, $"Cannot change {day.DayOfWeek}'s schedule because there are future appointments outside these working hours.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.DoctorName = doctor.User.FullName;

                return View(model);
            }

            List<DoctorSchedule> schedules =
                await _scheduleRepository.GetByDoctorIdAsync(model.DoctorId);

            foreach (DoctorScheduleDayViewModel day in model.Days)
            {
                DoctorSchedule? schedule =
                    schedules.FirstOrDefault(s => s.DayOfWeek == day.DayOfWeek);

                if (day.StartTime.HasValue && day.EndTime.HasValue)
                {
                    if (schedule == null)
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
                else if (schedule != null)
                {
                    await _scheduleRepository.DeleteAsync(schedule);
                }
            }

            TempData["SuccessMessage"] = "Schedule updated successfully.";

            return RedirectToAction(nameof(Manage), new {id = model.DoctorId});
        }
    }
}
