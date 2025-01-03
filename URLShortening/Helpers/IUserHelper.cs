using Microsoft.AspNetCore.Identity;
using URLShortening.Data;

namespace URLShortening.Helpers;

public interface IUserHelper
{
    public Task<bool> CreateUserAsync(User newUser, string password);
    public Task<string> GenerateEmailConfirmationTokenAsync(User user);
    public Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    public Task<string> GeneratePasswordResetTokenAsync(User user);

    public Task<IdentityResult> ResetPasswordAsync(User user,
        string token,
        string password);

    public Task<SignInResult> LoginAsync(string modelEmail,
        string modelPassword, bool modelRememberMe);

    public Task LogoutAsync();
    public Task<IdentityResult> UpdateUserAsync(User user);

    public Task<IdentityResult> ChangePasswordAsync(User user,
        string oldPassword, string newPassword);

    public Task<bool> CheckPasswordAsync(User user, string password);
    public Task<IdentityResult> DeleteUserAsync(User user);
    public Task<User?> FindUserByEmailAsync(string email);
    public Task<User?> FindUserByIdAsync(string id);
    Task<User?> FindUserByEmailIncludeUrlsAsync(string email);
}
