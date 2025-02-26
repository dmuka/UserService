using Core;
using Domain.Users;

namespace Domain.RefreshTokens;

public class RefreshToken : Entity
{
    public RefreshTokenId Id { get; private set; }
    public string Value { get; private set; }
    public DateTime ExpiresUtc { get; private set; }
    public User User { get; private set; }

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected RefreshToken() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class with specified user details.
    /// </summary>
    /// <param name="refreshTokenId">The unique identifier for the refresh token.</param>
    /// <param name="value">The refresh token value.</param>
    /// <param name="expiresUtc">The refresh token expire date.</param>
    /// <param name="user">The owner of the refresh token.</param>
    /// <exception cref="ArgumentException">Thrown when any string parameter is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when any object parameter is null.</exception>
    public static RefreshToken Create(
        Guid refreshTokenId,
        string value,
        DateTime expiresUtc,
        User user)
    {
        return new RefreshToken(
            new RefreshTokenId(refreshTokenId),
            value,
            expiresUtc, 
            user);
    }    
    
    private RefreshToken(
        RefreshTokenId refreshTokenId,
        string value,
        DateTime expiresUtc,
        User user)
    {
        ValidateRefreshTokenDetails(value, expiresUtc, user);

        Id = refreshTokenId;
        Value = value;
        ExpiresUtc = expiresUtc;
        User = user;
    }

    public void ChangeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Refresh token value can't be null or empty.", nameof(value));
        
        Value = value;
    }

    public void ChangeExpireDate(DateTime expiresUtc)
    {
        if (expiresUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expire date can't be less than current date.", nameof(expiresUtc));
        
        ExpiresUtc = expiresUtc;
    }
    
    /// <summary>
    /// Validates refresh token details.
    /// </summary>
    private static void ValidateRefreshTokenDetails(
        string value,
        DateTime expiresUtc,
        User user)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Refresh token value can't be null or empty.", nameof(value));
        if (expiresUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expire date can't be less than current date.", nameof(expiresUtc));
        ArgumentNullException.ThrowIfNull(user, nameof(user));
    }
}