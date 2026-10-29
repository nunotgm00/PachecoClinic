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
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Animal? animal = await _animalRepository.GetByIdWithClientAsync(id.Value);

            if(animal == null)
            {
                return NotFound();
            }

            return View(animal);
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

        [Authorize(Roles = "Admin,Employee,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Animal? animal = await _animalRepository.GetByIdAsync(id.Value);

            if (animal == null)
            {
                return NotFound();
            }

            EditAnimalsViewModel model = new EditAnimalsViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Species = animal.Species,
                Breed = animal.Breed,
                BirthDate = animal.BirthDate,
                Weight = animal.Weight,
                Color = animal.Color,
                Sex = animal.Sex,
                ClientId = animal.ClientId,
                CurrentImageURL = animal.ImageUrl,
                Clients = GetClients()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAnimalsViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }

            Animal? animal = await _animalRepository.GetByIdAsync(id);

            if(animal == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Clients = GetClients();
                model.CurrentImageURL = animal.ImageUrl;

                return View(model);
            }

            animal.Name = model.Name;
            animal.Species = model.Species;
            animal.Breed = model.Breed;
            animal.BirthDate = model.BirthDate;
            animal.Weight = model.Weight;
            animal.Color = model.Color;
            animal.Sex = model.Sex;
            animal.ClientId = animal.ClientId;

            if(model.ImageFile != null)
            {
                string extension = Path.GetExtension(model.ImageFile.FileName);

                string fileName = $"{Guid.NewGuid()}{extension}";

                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "animals");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath = Path.Combine(folder, fileName);

                using(FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(animal.ImageUrl))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        animal.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                animal.ImageUrl = $"/images/animals/{fileName}";
            }

            await _animalRepository.UpdateAsync(animal);

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
