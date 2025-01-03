using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using URLShortening.Data;

namespace URLShortening.Helpers;

public class UserHelper(
    DataContext dbContext,
    UserManager<User> userManager,
    SignInManager<User> signInManager) :
    IUserHelper
{
    public async Task<bool> CreateUserAsync(User newUser, string password)
    {
        var result = await userManager.CreateAsync(newUser, password);
        return result.Succeeded;
    }


    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        => await userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        => await userManager.ConfirmEmailAsync(user, token);

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
        => await userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<IdentityResult> ResetPasswordAsync(User user,
        string token,
        string password)
        => await userManager.ResetPasswordAsync(user, token, password);

    public async Task<SignInResult> LoginAsync(string modelEmail,
        string modelPassword, bool modelRememberMe)
        => await signInManager.PasswordSignInAsync(modelEmail, modelPassword,
            modelRememberMe, false);

    public async Task LogoutAsync()
        => await signInManager.SignOutAsync();

    public async Task<IdentityResult> UpdateUserAsync(User user)
        => await userManager.UpdateAsync(user);

    public async Task<IdentityResult> ChangePasswordAsync(User user,
        string oldPassword, string newPassword)
        => await userManager.ChangePasswordAsync(user, oldPassword,
            newPassword);

    public async Task<bool> CheckPasswordAsync(User user, string password)
        => await userManager.CheckPasswordAsync(user, password);

    public async Task<IdentityResult> DeleteUserAsync(User user)
        => await userManager.DeleteAsync(user);

    public async Task<User?> FindUserByEmailAsync(string email) =>
        await userManager.FindByEmailAsync(email);

    public async Task<User?> FindUserByIdAsync(string id) => await
        userManager.FindByIdAsync(id);

    public async Task<User?> FindUserByEmailIncludeUrlsAsync(string email)
    {
        return await dbContext.Users
            .Include(u => u.Urls)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
