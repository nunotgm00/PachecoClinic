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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMailHelper _mailHelper;

        public AccountController(
            IUserHelper userHelper,
            IClientRepository clientRepository,
            IDoctorRepository doctorRepository,
            IWebHostEnvironment webHostEnvironment,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _clientRepository = clientRepository;
            _doctorRepository = doctorRepository;
            _webHostEnvironment = webHostEnvironment;
            _mailHelper = mailHelper;
        }

        public IActionResult Login()
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
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
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user != null && user.ProfileCompleted && user.EmailConfirmed)
            {
                string token = await _userHelper.GeneratePasswordResetTokenAsync(user);

                string? resetLink = Url.Action("SetPassword", "Account", new
                {
                    email = user.Email,
                    token = token
                }, Request.Scheme);

                if (resetLink != null)
                {
                    _mailHelper.SendEmail(
                        user.Email!,
                        "Pacheco Clinic - Password Recovery",
                        $"<h1>Password Recovery</h1>" +
                        $"<p>A password reset has been requested for your account.</p>" +
                        $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(resetLink)}\">Reset Password</a></p>" +
                        $"<p>If you did not request this change, please ignore this email.</p>");
                }
            }

            TempData["SuccessMessage"] =
                "If an eligible account exists with this email, you will receive password reset instructions.";

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public async Task<IActionResult> SetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            User? user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.ProfileCompleted || !user.EmailConfirmed)
            {
                return BadRequest();
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

            if (!user.ProfileCompleted || !user.EmailConfirmed)
            {
                return BadRequest();
            }

            IdentityResult result = await _userHelper.ResetPasswordAsync(
                user, model.Token, model.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] = "Your password has been reset successfully.";

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
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

            IList<string> roles = await _userHelper.GetRolesAsync(user);

            CompleteRegistrationViewModel model = new CompleteRegistrationViewModel
            {
                Email = email,
                Token = token,
                Role = roles.FirstOrDefault() ?? string.Empty
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

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if(user == null)
            {
                return NotFound();
            }

            ProfileModelView model = new ProfileModelView
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModelView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if(user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if(model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath, "images", "users");

                Directory.CreateDirectory(uploadsFolder);

                string extension = Path.GetExtension(model.ImageFile.FileName);

                string fileName = $"{Guid.NewGuid()}{extension}";

                string filePath = Path.Combine(uploadsFolder, fileName);

                using(FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    string oldImagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath, user.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.ImageUrl = $"/images/users/{fileName}";
            }

            IdentityResult result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.ImageUrl = user.ImageUrl;

                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Profile));
        }
    }
}
