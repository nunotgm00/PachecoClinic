using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Helpers;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    public class AppointmentRequestsController : Controller
    {
        private readonly IAppointmentRequestRepository _requestRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserHelper _userHelper;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;

        public AppointmentRequestsController(
            IAppointmentRequestRepository requestRepository,
            IAppointmentRepository appointmentRepository,
            IClientRepository clientRepository,
            IUserHelper userHelper,
            IDoctorScheduleRepository doctorScheduleRepository)
        {
            _requestRepository = requestRepository;
            _appointmentRepository = appointmentRepository;
            _clientRepository = clientRepository;
            _userHelper = userHelper;
            _doctorScheduleRepository = doctorScheduleRepository;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            return View(_requestRepository.GetAllWithAppointmentDetails());
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyRequests()
        {
            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if(user == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByUserIdAsync(user.Id);

            if(client == null)
            {
                return NotFound();
            }

            IQueryable<AppointmentRequest> requests = _requestRepository
                .GetAllWithAppointmentDetails()
                .Where(r => r.Appointment.Animal.ClientId == client.Id);

            return View(requests);
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

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(int appointmentId)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);

            if(appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if(user == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByUserIdAsync(user.Id);

            if(client == null)
            {
                return NotFound();
            }

            if(appointment.Animal.ClientId != client.Id)
            {
                return Forbid();
            }

            if(appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest("Only scheduled appointments can be changed.");
            }

            DateTime appointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if(appointmentDateTime <= DateTime.Now)
            {
                return BadRequest("Past appointments cannot be changed.");
            }

            if(await _requestRepository.HasPendingRequestAsync(appointmentId))
            {
                return BadRequest("There is already a pending request for this appointment.");
            }

            CreateRequestAppointmentViewModel model = new CreateRequestAppointmentViewModel
            {
                AppointmentId = appointmentId
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestAppointmentViewModel model)
        {
            Appointment? appointment = await _appointmentRepository.GetByIdWithDetailsAsync(model.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            User? user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return NotFound();
            }

            Client? client = await _clientRepository.GetByUserIdAsync(user.Id);

            if (client == null)
            {
                return NotFound();
            }

            if (appointment.Animal.ClientId != client.Id)
            {
                return Forbid();
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                ModelState.AddModelError(string.Empty, "Only scheduled appointments can be changed.");
            }

            DateTime appointmentDateTime = appointment.Date.Date + appointment.StartTime;

            if (appointmentDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Past appointments cannot be changed.");
            }

            if (await _requestRepository.HasPendingRequestAsync(model.AppointmentId))
            {
                ModelState.AddModelError(string.Empty, "There is already a pending request for this appointment.");
            }

            if(model.Type == AppointmentRequestType.Reschedule)
            {
                if (!model.RequestedDate.HasValue)
                {
                    ModelState.AddModelError("RequestedDate", "Please select a new date.");
                }

                if (!model.RequestedStartTime.HasValue)
                {
                    ModelState.AddModelError("RequestedStartTime", "Please select a new time.");
                }

                if(model.RequestedDate.HasValue && model.RequestedStartTime.HasValue)
                {
                    DateTime requestedDateTime = model.RequestedDate.Value.Date + model.RequestedStartTime.Value;

                    if (requestedDateTime <= DateTime.Now)
                    {
                        ModelState.AddModelError("RequestedDate", "The requested appointment date and time must be in the future.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AppointmentRequest request = new AppointmentRequest
            {
                AppointmentId = model.AppointmentId,
                Type = model.Type!.Value,
                Status = AppointmentRequestStatus.Pending,
                Message = model.Message,
                CreatedAt = DateTime.Now,
            };

            if(model.Type == AppointmentRequestType.Reschedule)
            {
                request.RequestedDate = model.RequestedDate!.Value;

                request.RequestedStartTime = model.RequestedStartTime!.Value;
            }

            await _requestRepository.CreateAsync(request);

            return RedirectToAction(nameof(MyRequests));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            AppointmentRequest? request = await _requestRepository.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if(request.Status != AppointmentRequestStatus.Pending)
            {
                return BadRequest("Only pending requests can be approved.");
            }

            Appointment? appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            if(appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest("Only scheduled appointments can be changed.");
            }

            if(request.Type == AppointmentRequestType.Cancellation)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                request.Status = AppointmentRequestStatus.Approved;

                await _appointmentRepository.UpdateAsync(appointment);
                await _requestRepository.UpdateAsync(request);

                return RedirectToAction(nameof(Index));
            }

            if(!request.RequestedDate.HasValue || !request.RequestedStartTime.HasValue)
            {
                return BadRequest("A reschedule request must contain a date and start time.");
            }

            TimeSpan duration = appointment.EndTime - appointment.StartTime;

            DateTime requestedDate = request.RequestedDate.Value.Date;

            TimeSpan requestedStartTime = request.RequestedStartTime.Value;

            TimeSpan requestedEndTime = requestedStartTime + duration;

            DateTime requestedDateTime = requestedDate + requestedStartTime;

            if(requestedDateTime <= DateTime.Now)
            {
                return BadRequest("The requested appointment date and time must be in the future.");
            }

            bool doctorAvailable = await _doctorScheduleRepository.IsDoctorAvailableAsync(
                appointment.DoctorId,
                requestedDate.DayOfWeek,
                requestedStartTime,
                requestedEndTime);

            if (!doctorAvailable)
            {
                return BadRequest("The doctor is not available at the requested time.");
            }

            bool hasConflict = await _appointmentRepository.HasConflictAsync(
                appointment.DoctorId,
                appointment.RoomId,
                appointment.AnimalId,
                requestedDate,
                requestedStartTime,
                requestedEndTime,
                appointment.Id);

            if (hasConflict)
            {
                return BadRequest("The requested time conflicts with another appointment.");
            }

            appointment.Date = requestedDate;
            appointment.StartTime = requestedStartTime;
            appointment.EndTime = requestedEndTime;

            request.Status = AppointmentRequestStatus.Approved;

            await _appointmentRepository.UpdateAsync(appointment);
            await _requestRepository.UpdateAsync(request);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employe,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            AppointmentRequest? request = await _requestRepository.GetByIdAsync(id);

            if(request == null)
            {
                return NotFound();
            }

            if(request.Status != AppointmentRequestStatus.Pending)
            {
                return BadRequest("Only pending requests can be rejected.");
            }

            request.Status = AppointmentRequestStatus.Rejected;

            await _requestRepository.UpdateAsync(request);

            return RedirectToAction(nameof(Index));
        }
    }
}
