using URLShortening.Models;

namespace URLShortening.Services;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IMailService
{
    Task<ResponseResultModel> SendEmailAsync(string email, string subject,
        string body);
}
