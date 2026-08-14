using FluentValidation;
using VibeCheck.Service.Dtos.Auth;

namespace VibeCheck.Service.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().MinimumLength(3).MaximumLength(32)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);

        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(64);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.ResetToken).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
