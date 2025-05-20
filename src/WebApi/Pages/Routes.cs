namespace WebApi.Pages;

public static class Routes
{
    public const string CurrentIndex = "./Index";
    
    public const string SignIn = "/Account/SignIn";
    public const string SignUp = "/Account/SignUp";
    
    public const string SetupMfa = "/Account/SetupMfa";
    public const string Mfa = "/Account/Mfa";
    public const string MfaCreationConfirmation = "/Account/MfaCreationConfirmation";
    
    
    public const string ForgotPassword = "/Account/ForgotPassword";
    public const string ForgotPasswordConfirmation = "/Account/ForgotPasswordConfirmation";
    public const string ResetPassword = "/Account/ResetPassword";
    public const string ConfirmEmail = "/Account/ConfirmEmail";
    public const string ResendConfirmationEmail = "/Account/ResendConfirmationEmail";
    public const string ResetPasswordConfirmation = "/Account/ResetPasswordConfirmation";
    public const string Logout = "/Account/Logout";
    
    public const string Users = "/Users";
    public const string Roles = "/Roles";
    
    public const string Denied403 = "/AccessDenied";
    
    
}