using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly IGenericRepository<Specialty> _repository;
        private readonly IDoctorRepository _doctorRepository;

        public SpecialtiesController(IGenericRepository<Specialty> repository, IDoctorRepository doctorRepository)
        {
            _repository = repository;
            _doctorRepository = doctorRepository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.GetAll());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Specialty? specialty = await _repository.GetByIdAsync(id.Value);

            if(specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specialty specialty)
        {
            if (!ModelState.IsValid)
            {
                return View(specialty);
            }

            await _repository.CreateAsync(specialty);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Specialty? specialty = await _repository.GetByIdAsync(id.Value);

            if(specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Specialty specialty)
        {
            if(id != specialty.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(specialty);
            }

            await _repository.UpdateAsync(specialty);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Specialty? specialty = await _repository.GetByIdAsync(id.Value);

            if( specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Specialty? specialty = await _repository.GetByIdAsync(id);

            if(specialty == null)
            {
                return NotFound();
            }

            bool hasDoctors = await _doctorRepository.HasDoctorsBySpecialtyAsync(id);

            if (hasDoctors)
            {
                TempData["ErrorMessage"] = "This specialty cannot be deleted because it has doctors associated with it.";

                return RedirectToAction(nameof(Details), new { id });
            }

            await _repository.DeleteAsync(specialty);

            return RedirectToAction(nameof(Index));
        }
    }
}
