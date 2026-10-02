using Microsoft.AspNetCore.Identity;
using PachecoClinic.Data.Entities;

namespace PachecoClinic.Helpers
{
    public interface IUserHelper
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);

        Task LogoutAsync();

        Task<bool> IsUserInRoleAsync(User user, string roleName);
    }
}
