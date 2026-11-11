using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


    }
}
