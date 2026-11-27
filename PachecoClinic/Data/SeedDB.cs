using Microsoft.AspNetCore.Identity;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Data
{
    public class SeedDB
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public SeedDB(
            DataContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await CheckRoleAsync("Admin");
            await CheckRoleAsync("Employee");
            await CheckRoleAsync("Doctor");
            await CheckRoleAsync("Client");

            User? admin = await _userManager.FindByEmailAsync("AdminPacheco@Pclinic.com");

            if (admin == null)
            {
                string? password = _configuration["SeedAdmin:Password"];
                admin = new User
                {
                    FirstName = "Admin",
                    LastName = "PachecoClinic",
                    Email = "AdminPacheco@Pclinic.com",
                    UserName = "AdminPacheco@Pclinic.com",
                    EmailConfirmed = true,
                    ProfileCompleted = true
                };

                IdentityResult result =
                    await _userManager.CreateAsync(admin, password);

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
