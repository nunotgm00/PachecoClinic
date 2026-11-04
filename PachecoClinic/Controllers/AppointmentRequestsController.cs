using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;

namespace PachecoClinic.Controllers
{
    public class AppointmentRequestsController : Controller
    {
        private readonly IAppointmentRequestRepository _requestRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserHelper _userHelper;

        public AppointmentRequestsController(
            IAppointmentRequestRepository requestRepository,
            IAppointmentRepository appointmentRepository,
            IClientRepository clientRepository,
            IUserHelper userHelper)
        {
            _requestRepository = requestRepository;
            _appointmentRepository = appointmentRepository;
            _clientRepository = clientRepository;
            _userHelper = userHelper;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            return View(_requestRepository.GetAllWithAppointmentDetails());
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            AppointmentRequest? request = await _requestRepository.GetByIdWithAppointmentDetailsAsync(id.Value);

            if(request == null)
            {
                return NotFound();
            }

            return View(request);
        }
    }
}
