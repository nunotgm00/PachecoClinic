using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly IAnimalRepository _animalRepository;
        private readonly IClientRepository _clientRepository;

        public AnimalsController(IAnimalRepository animalRepository, IClientRepository clientRepository)
        {
            _animalRepository = animalRepository;
            _clientRepository = clientRepository;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public ActionResult Index()
        {
            return View(_animalRepository.GetAllWithClients());
        }

        private IEnumerable<SelectListItem> GetClients()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach(Client client in _clientRepository.GetAllWithUsers())
            {
                items.Add(new SelectListItem
                {
                    Value = client.Id.ToString(),
                    Text = client.User.FullName
                });
            }

            return items;
        }
    }
}
