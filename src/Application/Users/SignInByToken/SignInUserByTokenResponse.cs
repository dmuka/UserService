namespace Application.Users.SignInByToken;

public sealed record SignInUserByTokenResponse(string AccessToken, string RefreshToken);