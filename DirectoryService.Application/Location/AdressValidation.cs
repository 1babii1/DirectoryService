using DirectoryService.Contracts.Location;
using FluentValidation;

namespace DirectoryService.Application.Location;

public class AdressValidation : AbstractValidator<Address>
{
    public AdressValidation()
    {
        RuleFor(x => x.City).NotEmpty().NotNull();
        RuleFor(x => x.Country).NotEmpty().NotNull();
        RuleFor(x => x.Street).NotEmpty().NotNull();
    }
}