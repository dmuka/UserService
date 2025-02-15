namespace Application.Abstractions.Authentication;

/// <summary>
/// Provides methods for hashing passwords and verifying password hashes.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a hash for the specified password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hash representation of the password.</returns>
    string GetHash(string password);

    /// <summary>
    /// Verifies that a password matches the specified hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="passwordHash">The hash to compare against.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
    bool CheckPassword(string password, string passwordHash);
}
