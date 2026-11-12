using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;
using System.Threading.Tasks;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IGenericRepository<Specialty> _specialtyRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IClientRepository _clientRepository;

        public UsersController(
            IUserHelper userHelper,
            IGenericRepository<Specialty> specialtyRepository,
            IDoctorRepository doctorRepository,
            IClientRepository clientRepository)
        {
            _userHelper = userHelper;
            _specialtyRepository = specialtyRepository;
            _doctorRepository = doctorRepository;
            _clientRepository = clientRepository;
        }

        public async Task<IActionResult> Index()
        {
            List<UserIndexViewModel> model = new List<UserIndexViewModel>();

            foreach(User user in _userHelper.GetAllUsers())
            {
                IList<string> roles = await _userHelper.GetRolesAsync(user);

                model.Add(new UserIndexViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "-",
                    ProfileCompleted = user.ProfileCompleted,
                });
            }

            return View(model);
         }

        public IActionResult Create()
        {
            CreateUserViewModel model = new CreateUserViewModel
            {
                Roles = GetRoles(),
                Specialties = GetSpecialties()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            model.Roles = GetRoles();
            model.Specialties = GetSpecialties();

            List<string> allowedRoles;

            if (User.IsInRole("Admin"))
            {
                allowedRoles = new List<string>
                {
                    "Admin",
                    "Employee",
                    "Doctor",
                    "Client"
                };
            }
            else
            {
                allowedRoles = new List<string>
                {
                    "Doctor",
                    "Client"
                };
            }

            if(string.IsNullOrEmpty(model.Role) || !allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError("Role", "Invalid role.");
            }

            if(model.Role == "Doctor" && !model.SpecialtyId.HasValue)
            {
                ModelState.AddModelError("SpecialtyId", "A specialty is required for a doctor");
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                User? existingUser = await _userHelper.GetUserByEmailAsync(model.Email);

                if(existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = "Pending",
                LastName = "Registration",
                ProfileCompleted = false,
                EmailConfirmed = false
            };

            IdentityResult result = await _userHelper.AddUserAsync(user);

            if (!result.Succeeded)
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userHelper.AddUserToRoleAsync(user, model.Role);

            if(model.Role == "Doctor")
            {
                Doctor doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecialtyId = model.SpecialtyId!.Value,
                    Active = false
                };

                await _doctorRepository.CreateAsync(doctor);
            }
            else if(model.Role == "Client")
            {
                Client client = new Client
                {
                    UserId = user.Id,
                };

                await _clientRepository.CreateAsync(client);
            }

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetRoles()
        {
            List<SelectListItem> roles = new List<SelectListItem>();

            if (User.IsInRole("Admin"))
            {
                roles.Add(new SelectListItem
                {
                    Value = "Admin",
                    Text = "Admin"
                });

                roles.Add(new SelectListItem
                {
                    Value = "Employee",
                    Text = "Employee"
                });
            }

            roles.Add(new SelectListItem
            {
                Value = "Doctor",
                Text = "Doctor"
            });

            roles.Add(new SelectListItem
            {
                Value = "Client",
                Text = "Client"
            });

            return roles;
        }

        private IEnumerable<SelectListItem> GetSpecialties()
        {
            List<SelectListItem> specialties = new List<SelectListItem>();

            foreach(Specialty specialty in _specialtyRepository.GetAll())
            {
                specialties.Add(new SelectListItem
                {
                    Value = specialty.Id.ToString(),
                    Text = specialty.Name
                });
            }

            return specialties;
        }
    }
}
