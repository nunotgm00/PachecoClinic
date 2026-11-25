using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly IGenericRepository<Room> _repository;
        private readonly IAppointmentRepository _appointmentRepository;

        public RoomsController(IGenericRepository<Room> repository, IAppointmentRepository appointmentRepository)
        {
            _repository = repository;
            _appointmentRepository = appointmentRepository;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            return View(_repository.GetAll());
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room? room = await _repository.GetByIdAsync(id.Value);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                return View(room);
            }

            await _repository.CreateAsync(room);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room? room = await _repository.GetByIdAsync(id.Value);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(room);
            }

            if (room.UnderMaintenance && await _appointmentRepository.HasFutureAppointmentsByRoomAsync(room.Id))
            {
                ModelState.AddModelError(
                    nameof(room.UnderMaintenance),
                    "This room cannot be placed under maintenance because it has future appointments scheduled.");

                return View(room);
            }

            await _repository.UpdateAsync(room);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room? room = await _repository.GetByIdAsync(id.Value);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Room? room = await _repository.GetByIdAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            bool hasAppointments = await _appointmentRepository.HasAppointmentByRoomAsync(id);

            if (hasAppointments)
            {
                TempData["ErrorMessage"] = "This room cannot be deleted because it has appointment history.";

                return RedirectToAction(nameof(Details), new { id });
            }

            await _repository.DeleteAsync(room);

            return RedirectToAction(nameof(Index));
        }
    }
}
