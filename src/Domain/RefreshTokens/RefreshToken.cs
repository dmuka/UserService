using Core;
using Domain.RefreshTokens.Specifications;
using Domain.Specifications;
using Domain.Users;
using Domain.Users.Specifications;

namespace Domain.RefreshTokens;

public class RefreshToken : Entity
{
    public new RefreshTokenId Id { get; private set; }
    public string Value { get; private set; }
    public DateTime ExpiresUtc { get; private set; }
    public UserId UserId { get; private set; }

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
    /// <param name="userId">The id of the owner of the refresh token.</param>
    /// <exception cref="ArgumentException">Thrown when any string parameter is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when any object parameter is null.</exception>
    public static Result<RefreshToken> Create(
        Guid refreshTokenId,
        string value,
        DateTime expiresUtc,
        UserId userId)
    {
        var resultsWithFailures = ValidateRefreshTokenDetails(value, expiresUtc, userId);

        if (resultsWithFailures.Length != 0)
        {
            return Result<RefreshToken>.ValidationFailure(ValidationError.FromResults(resultsWithFailures));
        }

        return new RefreshToken(
            refreshTokenId,
            value,
            expiresUtc, 
            userId);
    }    
    
    private RefreshToken(
        Guid refreshTokenId,
        string value,
        DateTime expiresUtc,
        UserId userId)
    {
        Id = new RefreshTokenId(refreshTokenId);
        Value = value;
        ExpiresUtc = expiresUtc;
        UserId = userId;
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
    private static Result[] ValidateRefreshTokenDetails(
        string value,
        DateTime expiresUtc,
        UserId userId)
    {
        var validationResults = new []
        {
            new MustBeNonNullNonEmpty(value).IsSatisfied(),
            new ExpirationDateMustBeInFuture(expiresUtc).IsSatisfied(),
            new MustBeNonNullValue<UserId>(userId).IsSatisfied()
        };
            
        var results = validationResults.Where(result => result.IsFailure);

        return results.ToArray();
    }
}