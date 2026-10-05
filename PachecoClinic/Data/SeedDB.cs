using Microsoft.AspNetCore.Identity;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class SeedDB
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDB(
            DataContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await CheckRoleAsync("Admin");
            await CheckRoleAsync("Employee");
            await CheckRoleAsync("Doctor");
            await CheckRoleAsync("Client");

            User? admin = await _userManager.FindByEmailAsync("admin@pachecoClinic.pt");

            if (admin == null)
            {
                admin = new User
                {
                    FirstName = "Admin",
                    LastName = "PachecoClinic",
                    Email = "admin@pachecoClinic.pt",
                    UserName = "admin@pachecoClinic.pt",
                    EmailConfirmed = true
                };

                IdentityResult result =
                    await _userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        private async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(roleName));
            }
        }
    }
}
