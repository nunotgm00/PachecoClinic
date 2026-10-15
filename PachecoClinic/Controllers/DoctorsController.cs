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
