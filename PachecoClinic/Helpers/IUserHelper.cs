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

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        Task<User?> GetUserByIdAsync(string id);

        Task<IdentityResult> UpdatedUserAsync(User user);
    }
}
