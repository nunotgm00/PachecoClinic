using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IGenericRepository<Specialty> _specialtyRepository;
        private readonly IUserHelper _userHelper;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMailHelper _mailHelper;

        public DoctorsController(
            IDoctorRepository doctorRepository,
            IGenericRepository<Specialty> specialtyRepository,
            IUserHelper userHelper,
            IAppointmentRepository appointmentRepository,
            IMailHelper mailHelper)
        {
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
            _userHelper = userHelper;
            _appointmentRepository = appointmentRepository;
            _mailHelper = mailHelper;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_doctorRepository.GetAllWithUsersAndSpecialties());
        }

        public IActionResult Create()
        {
            CreateDoctorViewModel model = new CreateDoctorViewModel();

            model.Specialties = GetSpecialties();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Specialties = GetSpecialties();
                return View(model);
            }

            User? existingUser = await _userHelper.GetUserByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "There is already a user with this email.");

                model.Specialties = GetSpecialties();

                return View(model);
            }

            User user = new User
            {
                FirstName = "Pending",
                LastName = "Registration",
                Email = model.Email,
                UserName = model.Email,
                EmailConfirmed  = false,
                ProfileCompleted = false
            };

            IdentityResult userResult = await _userHelper.AddUserAsync(user);

            if (!userResult.Succeeded)
            {
                foreach(IdentityError error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.Specialties = GetSpecialties();

                return View(model);
            }

            IdentityResult roleResult = await _userHelper.AddUserToRoleAsync(user, "Doctor");

            if (!roleResult.Succeeded)
            {
                foreach (IdentityError error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.Specialties = GetSpecialties();

                return View(model);
            }

            Doctor doctor = new Doctor
            {
                UserId = user.Id,
                SpecialtyId = model.SpecialtyId!.Value,
                Active = false
            };

            await _doctorRepository.CreateAsync(doctor);

            string token = await _userHelper.GeneratePasswordResetTokenAsync(user);

            string? registrationLink = Url.Action("CompleteRegistration", "Account", new
            {
                email = user.Email,
                token = token
            }, Request.Scheme);

            Response response = _mailHelper.SendEmail(
                user.Email!,
                "Pacheco Clinic - Complete Registration",
                $"<h1>Complete Registration</h1>" +
                $"<p>An account has been created for you at Pacheco Clinic.</p>" +
                $"<p>To complete your registration and create your password, click the link below:</p>" +
                $"<p><a href=\"{registrationLink}\">Complete Registration</a></p>");

            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] =
                    "Doctor created successfully. Registration email sent.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Doctor created successfully, but the registration email could not be sent.";
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id.Value);

            if(doctor == null)
            {
                return NotFound();
            }

            List<DoctorAppointmentScheduleViewModel> appointments =
                _appointmentRepository.GetAllWithDetails()
                .Where(a => a.DoctorId == doctor.Id &&
                    a.Status != AppointmentStatus.Cancelled)
                .Select(a => new DoctorAppointmentScheduleViewModel
                {
                    Id = a.Id,
                    Subject = a.Animal.Name + " - " + a.Motive,
                    StartTime = a.Date.Date + a.StartTime,
                    EndTime = a.Date.Date + a.EndTime,
                    Description = a.Motive,
                    Location = a.Room.Name
                })
                .ToList();

            ViewBag.ScheduleData = appointments;

            return View(doctor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id.Value);

            if (doctor == null)
            {
                return NotFound();
            }

            EditDoctorViewModel model = new EditDoctorViewModel
            {
                Id = doctor.Id,
                UserId = doctor.UserId,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Email = doctor.User.Email!,
                PhoneNumber = doctor.User.PhoneNumber,
                SpecialtyId = doctor.SpecialtyId,
                Active = doctor.Active,
                Specialties = GetSpecialties()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDoctorViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Specialties = GetSpecialties();
                return View(model);
            }

            Doctor? doctor = await _doctorRepository.GetByIdAsync(id);

            if(doctor == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByIdAsync(doctor.UserId);

            if (user == null)
            {
                return NotFound();
            }

            User? existingUser = await _userHelper.GetUserByEmailAsync(model.Email);

            if(existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Email", "There is already a user with this email.");

                model.Specialties = GetSpecialties();

                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            IdentityResult userResult = await _userHelper.UpdateUserAsync(user);

            if (!userResult.Succeeded)
            {
                foreach(IdentityError error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.Specialties = GetSpecialties();

                return View(model);
            }

            doctor.SpecialtyId = model.SpecialtyId;
            doctor.Active = model.Active;

            await _doctorRepository.UpdateAsync(doctor);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Doctor? doctor = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id.Value);

            if(doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Doctor? doctor = await _doctorRepository.GetByIdAsync(id);

            if(doctor == null)
            {
                return NotFound();
            }

            if(await _doctorRepository.HasAppointmentAsync(id))
            {
                Doctor? doctorWithData = await _doctorRepository.GetByIdWithUserAndSpecialtyAsync(id);

                ModelState.AddModelError(string.Empty,
                    "This Doctor cannot be deleted because there are appointments associated with them. " +
                    "You can deactivate this doctor by editing it.");

                return View("Delete", doctorWithData);
            }

            User? user = await _userHelper.GetUserByIdAsync(doctor.UserId);

            await _doctorRepository.DeleteAsync(doctor);

            if (user != null)
            {
                await _userHelper.DeleteUserAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetSpecialties()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach(Specialty specialty in _specialtyRepository.GetAll())
            {
                items.Add(new SelectListItem
                {
                    Value = specialty.Id.ToString(),
                    Text = specialty.Name
                });
            }

            return items;
        }
    }
}
