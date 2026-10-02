using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PachecoClinic.Data;
using PachecoClinic.Data.Entities;
using PachecoClinic.Data.Helpers;
using System.Threading.Tasks;

namespace PachecoClinic
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            builder.Services.AddScoped<IUserHelper, UserHelper>();

            builder.Services.AddTransient<SeedDB>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using(IServiceScope scope = app.Services.CreateScope())
            {
                SeedDB seeder = scope.ServiceProvider.GetRequiredService<SeedDB>();
                await seeder.SeedAsync();
            }

            app.Run();
        }
    }
}
