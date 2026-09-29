namespace SMC.Master.Infrastructure.Authentication;

public static class PasswordService
{
    public static string Hash(string plainText) => BCrypt.Net.BCrypt.HashPassword(plainText);

    public static bool Verify(string plainText, string hash) => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
