namespace URLShortening.Helpers;

public interface ICodeGeneratorHelper
{
    public string GenerateConfirmationCode(string input, string secretKey);
}
