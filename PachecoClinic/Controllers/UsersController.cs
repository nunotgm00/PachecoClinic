using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;

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

        public IActionResult Index()
        {
            IQueryable<User> users = _userHelper.GetAllUsers();

            return View(users);
         }


    }
}
