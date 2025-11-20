using System.Text.RegularExpressions;
using DirectoryService.Contracts.Location;
using FluentValidation;

namespace DirectoryService.Application.Location;

public class CreateLocationValidation : AbstractValidator<CreateLocationRequest>
{
    private static readonly Regex IanaTimezoneRegex = new Regex(@"^[A-Za-z_]+\/[A-Za-z_]+$");

    public CreateLocationValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 120).WithMessage("Name is not valid. It must be between 3 and 120 characters");

        RuleFor(x => x.Address)
            .NotNull()
            .SetValidator(new AdressValidation()).WithMessage("Address is not valid");

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .Must(BeAValidIanaTimezone)
            .WithMessage("Timezone must be a valid IANA timezone ID.");
    }

    private bool BeAValidIanaTimezone(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return false;

        return IanaTimezoneRegex.IsMatch(timezone);
    }
}