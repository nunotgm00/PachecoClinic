using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserHelper _userHelper;

        public ClientsController(IClientRepository clientRepository, IUserHelper userHelper)
        {
            _clientRepository = clientRepository;
            _userHelper = userHelper;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            return View(_clientRepository.GetAllWithUsers());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            CreateClientViewModel model = new CreateClientViewModel();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? existingUser = await _userHelper.GetUserByEmailAsync(model.Email);

            if(existingUser != null)
            {
                ModelState.AddModelError("Email", "There is already a user with this email.");

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

                return View(model);
            }

            IdentityResult roleResult = await _userHelper.AddUserToRoleAsync(user, "Client");

            if (!roleResult.Succeeded)
            {
                foreach(IdentityError error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            Client client = new Client
            {
                UserId = user.Id,
                Address = model.Address,
                Nif = model.Nif,
            };

            await _clientRepository.CreateAsync(client);

            return RedirectToAction("Index");
        }
    }
}
