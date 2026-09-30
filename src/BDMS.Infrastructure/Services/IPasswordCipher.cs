using System.Security.Cryptography;
using System.Text;
using BDMS.Application.Services;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Byte-for-byte port of Login_Module/LoginPageNew.aspx.cs Encrypt()/Decrypt(): AES with a key
/// derived via Rfc2898DeriveBytes (PBKDF1-style, .NET's default HMACSHA1/1000 iterations) from a
/// hardcoded key string and a hardcoded salt ("Ivan Medvedev" as bytes). This is reversible
/// encryption, not a one-way hash — kept exactly as legacy does it so existing Login_Details
/// passwords would still verify if ever imported. This is a real security weakness in the
/// original system; see the note on Models/User.cs for the recommended hardening path.
/// </summary>
public class LegacyPasswordCipher : IPasswordCipher
{
    private const string EncryptionKey = "MAKV2SPBNI12345";
    private static readonly byte[] Salt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }; // "Ivan Medvedev"

    public string Encrypt(string plainText)
    {
        var clearBytes = Encoding.Unicode.GetBytes(plainText.Trim());
        using var aes = Aes.Create();
#pragma warning disable SYSLIB0041 // legacy compatibility: matches original Rfc2898DeriveBytes(string, byte[]) overload exactly
        using var pdb = new Rfc2898DeriveBytes(EncryptionKey, Salt);
#pragma warning restore SYSLIB0041
        aes.Key = pdb.GetBytes(32);
        aes.IV = pdb.GetBytes(16);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(clearBytes, 0, clearBytes.Length);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
#pragma warning disable SYSLIB0041
        using var pdb = new Rfc2898DeriveBytes(EncryptionKey, Salt);
#pragma warning restore SYSLIB0041
        aes.Key = pdb.GetBytes(32);
        aes.IV = pdb.GetBytes(16);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(cipherBytes, 0, cipherBytes.Length);
        }
        return Encoding.Unicode.GetString(ms.ToArray());
    }
}
