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

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByIdWithUserAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            EditClientViewModel model = new EditClientViewModel
            {
                Id = client.Id,
                FirstName = client.User.FirstName,
                LastName = client.User.LastName,
                Email = client.User.Email!,
                PhoneNumber = client.User.PhoneNumber,
                Address = client.Address,
                Nif = client.Nif
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditClientViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Client? client = await _clientRepository.GetByIdAsync(id);

            if(client == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByIdAsync(client.UserId);

            if(user == null)
            {
                return NotFound();
            }

            User? existingUser = await _userHelper.GetUserByEmailAsync(model.Email);

            if(existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Email", "There is already a user with this email");

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

                return View(model);
            }

            client.Address = model.Address;
            client.Nif = model.Nif;

            await _clientRepository.UpdateAsync(client);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByIdWithUserAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Client? client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            if(await _clientRepository.HasAnimalsAsync(id))
            {
                Client? clientWithUser = await _clientRepository.GetByIdWithUserAsync(id);

                ModelState.AddModelError(string.Empty,
                    "This client cannot be deleted because there are animals associated with them.");

                return View("Delete", clientWithUser);
            }

            User? user = await _userHelper.GetUserByIdAsync(client.UserId);

            await _clientRepository.DeleteAsync(client);

            if(user != null)
            {
                await _userHelper.DeleteUserAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
