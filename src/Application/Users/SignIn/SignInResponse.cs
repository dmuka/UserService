namespace Application.Users.SignIn;

public sealed record SignInResponse(string AccessToken, Guid SessionId);