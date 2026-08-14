using FluentValidation;
using VibeCheck.Service.Dtos.VibeChecks;

namespace VibeCheck.Service.Validators;

public class CreateVibeCheckRequestValidator : AbstractValidator<CreateVibeCheckRequest>
{
    public CreateVibeCheckRequestValidator()
    {
        RuleFor(x => x.VenueId).NotEmpty();
        RuleFor(x => x.VibeScore).InclusiveBetween(1, 5);
        RuleFor(x => x.CrowdLevel).IsInEnum();
        RuleFor(x => x.Comment).MaximumLength(500);
        RuleFor(x => x.PhotoUrls).Must(p => p == null || p.Count <= 5).WithMessage("Up to 5 photos per vibe check.");
        RuleForEach(x => x.PhotoUrls).MaximumLength(500);
    }
}

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
    }
}
