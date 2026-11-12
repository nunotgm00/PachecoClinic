using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IClientRepository _clientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public AccountController(IUserHelper userHelper, IClientRepository clientRepository, IDoctorRepository doctorRepository)
        {
            _userHelper = userHelper;
            _clientRepository = clientRepository;
            _doctorRepository = doctorRepository;
        }

        public IActionResult Login()
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userHelper.LoginAsync(model.Email, model.Password, model.RememberMe);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password");

            return View(model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> SetPassword(string email, string token)
        {
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            User? user = await _userHelper.GetUserByEmailAsync(email);

            if(user == null)
            {
                return NotFound();
            }

            SetPasswordViewModel model = new SetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

            if (!result.Succeeded)
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] = "Your password has been set successfully.";

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            User? user = await _userHelper.GetUserByEmailAsync(email);

            if(user == null)
            {
                return NotFound();
            }

            if (user.ProfileCompleted)
            {
                return RedirectToAction(nameof(Login));
            }

            CompleteRegistrationViewModel model = new CompleteRegistrationViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRegistration(CompleteRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userHelper.GetUserByEmailAsync(model.Email);

            if(user == null)
            {
                return NotFound();
            }

            if (user.ProfileCompleted)
            {
                return RedirectToAction(nameof(Login));
            }

            IList<string> roles = await _userHelper.GetRolesAsync(user);

            Client? client = null;
            Doctor? doctor = null;

            if (roles.Contains("Client"))
            {
                client = await _clientRepository.GetByUserIdAsync(user.Id);

                if(client == null)
                {
                    return NotFound();
                }
            }

            if (roles.Contains("Doctor"))
            {
                doctor = await _doctorRepository.GetByUserIdAsync(user.Id);

                if (doctor == null)
                {
                    return NotFound();
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            IdentityResult updateResult = await _userHelper.UpdateUserAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach(IdentityError error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            IdentityResult passwordResult = await _userHelper.ResetPasswordAsync(
                user, model.Token, model.Password);

            if (!passwordResult.Succeeded)
            {
                foreach(IdentityError error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            user.ProfileCompleted = true;
            user.EmailConfirmed = true;

            IdentityResult finalUpdateResult = await _userHelper.UpdateUserAsync(user);

            if (!finalUpdateResult.Succeeded)
            {
                foreach(IdentityError error in finalUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            if(doctor != null)
            {
                doctor.Active = true;

                await _doctorRepository.UpdateAsync(doctor);
            }

            if (client != null)
            {
                client.Address = model.Address;
                client.Nif = model.Nif;

                await _clientRepository.UpdateAsync(client);
            }

            TempData["SuccessMessage"] = "Registration completed successfully. You can now log in.";

            return RedirectToAction(nameof(Login));
        }
    }
}
