using FluentValidation;
using VibeCheck.Service.Dtos.Users;

namespace VibeCheck.Service.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Bio).MaximumLength(280);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
    }
}
