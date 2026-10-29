using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Models;

namespace PachecoClinic.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly IAnimalRepository _animalRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AnimalsController(
            IAnimalRepository animalRepository,
            IClientRepository clientRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _animalRepository = animalRepository;
            _clientRepository = clientRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public ActionResult Index()
        {
            return View(_animalRepository.GetAllWithClients());
        }

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public IActionResult Create()
        {
            CreateAnimalViewModel model = new CreateAnimalViewModel
            {
                Clients = GetClients()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAnimalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = GetClients();

                return View(model);
            }

            string extension = Path.GetExtension(model.ImageFile.FileName);

            string fileName = $"{Guid.NewGuid()}{extension}";

            string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "animals");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filePath = Path.Combine(folder, fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            Animal animal = new Animal
            {
                Name = model.Name,
                Species = model.Species,
                Breed = model.Breed,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                Color = model.Color,
                Sex = model.Sex,
                ClientId = model.ClientId,
                ImageUrl = $"/images/animals/{fileName}"
            };

            await _animalRepository.CreateAsync(animal);

            return RedirectToAction(nameof(Index));
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
