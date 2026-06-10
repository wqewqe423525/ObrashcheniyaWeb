using System.Security.Cryptography;

namespace ObrashcheniyaWeb.Security;

/// <summary>
/// Хэширование и проверка паролей. Реализует рекомендацию из раздела 3.3
/// диплома: хранить не открытый пароль, а его необратимый хэш.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}

/// <summary>
/// PBKDF2 (HMAC-SHA256). Формат хранимой строки:
/// {итерации}.{salt-base64}.{hash-base64}.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // 128 бит
    private const int KeySize = 32;        // 256 бит
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const char Delimiter = '.';

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var segments = storedHash.Split(Delimiter);
        if (segments.Length != 3)
            return false;

        if (!int.TryParse(segments[0], out var iterations))
            return false;

        byte[] salt, expectedHash;
        try
        {
            salt = Convert.FromBase64String(segments[1]);
            expectedHash = Convert.FromBase64String(segments[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations, Algorithm, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
