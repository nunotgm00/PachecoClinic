using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Controllers
{
    [Authorize]
    public class InterventionsController : Controller
    {
        private readonly IInterventionRepository _interventionRepository;

        public InterventionsController(IInterventionRepository interventionRepository)
        {
            _interventionRepository = interventionRepository;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Index()
        {
            IQueryable<Intervention> interventions = _interventionRepository.GetAllWithDetails();

            return View(interventions);
        }
    }
}
