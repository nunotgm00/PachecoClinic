using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Helpers;
using PachecoClinic.Models;
using System.Threading.Tasks;

namespace PachecoClinic.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class NotificationsController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMailHelper _mailHelper;

        public NotificationsController(IClientRepository clientRepository,
            IMailHelper mailHelper)
        {
            _clientRepository = clientRepository;
            _mailHelper = mailHelper;
        }

        public IActionResult ClinicClosure()
        {
            ClinicClosureViewModel model = new ClinicClosureViewModel
            {
                Date = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClinicClosure(ClinicClosureViewModel model)
        {
            if (model.Date < DateTime.Today)
            {
                ModelState.AddModelError("Date", "The closure date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var clients = _clientRepository.GetAllWithUsers();

            int sent = 0;
            int failed = 0;

            foreach (var client in clients)
            {
                if (!string.IsNullOrEmpty(client.User.Email))
                {
                    Response response = _mailHelper.SendEmail(
                    client.User.Email,
                    "Pacheco Clinic - Clinic Closure",
                    $"<h1>Clinic Closure</h1>" +
                    $"<p>Hello {client.User.FirstName},</p>" +
                    $"<p>Pacheco Clinic will be closed on <strong>{model.Date:dd/MM/yyyy}</strong>.</p>" +
                    $"<p>{model.Message}</p>");

                    if (response.IsSuccess)
                    {
                        sent++;
                    }
                    else
                    {
                        failed++;
                    }

                }
            }

            TempData["SuccessMessage"] = $"Notification sent to {sent} client(s)";

            if(failed > 0)
            {
                TempData["ErrorMessage"] = $"{failed} email(s) could not be sent";
            }

            return RedirectToAction(nameof(ClinicClosure));
        }
    }
}
