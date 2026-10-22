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

        public DoctorsController(
            IDoctorRepository doctorRepository,
            IGenericRepository<Specialty> specialtyRepository,
            IUserHelper userHelper)
        {
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
            _userHelper = userHelper;
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
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            string temporaryPassword = $"Temp1!{Guid.NewGuid():N}";

            IdentityResult userResult = await _userHelper.AddUserAsync(user, temporaryPassword);

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
                SpecialtyId = model.SpecialtyId,
                Active = model.Active
            };

            await _doctorRepository.CreateAsync(doctor);

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
