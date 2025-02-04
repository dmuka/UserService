using FluentValidation;

namespace Application.Users.SignUp;

internal sealed class SignUpUserCommandValidator : AbstractValidator<SignUpUserCommand>
{
    public SignUpUserCommandValidator()
    {
        RuleFor(c => c.Username).NotEmpty();
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
