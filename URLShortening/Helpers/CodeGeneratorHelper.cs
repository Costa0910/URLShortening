using System.Security.Cryptography;
using System.Text;

namespace URLShortening.Helpers;

public class CodeGeneratorHelper : ICodeGeneratorHelper
{
    public string GenerateConfirmationCode(string input, string secretKey)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(secretKey))
            throw new ArgumentException(
                "Input and secretKey cannot be null or empty.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        var base64 = Convert.ToBase64String(hash)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        return base64.Length > 10 ? base64[..10] : base64;
    }
}
