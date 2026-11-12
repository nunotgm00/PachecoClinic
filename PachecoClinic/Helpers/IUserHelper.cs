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

        Task<IdentityResult> AddUserAsync(User user);

        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        Task<User?> GetUserByIdAsync(string id);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> DeleteUserAsync(User user);

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<IdentityResult> ResetPasswordAsync(
            User user,
            string token,
            string password);

        IQueryable<User> GetAllUsers();

        Task<IList<string>> GetRolesAsync(User user);
    }
}
