using FluentValidation;
using VibeCheck.Service.Dtos.Venues;

namespace VibeCheck.Service.Validators;

public class CreateVenueRequestValidator : AbstractValidator<CreateVenueRequest>
{
    public CreateVenueRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}

public class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
{
    public UpdateVenueRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}
