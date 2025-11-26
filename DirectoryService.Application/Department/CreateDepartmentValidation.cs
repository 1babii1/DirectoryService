using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Department;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Department;

public class CreateDepartmentValidation : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidation()
    {
        RuleFor(x => x.Name).MustBeValueObject(name => DepartmentName.Create(name.Value));
        RuleFor(x => x.Identifier).MustBeValueObject(identifier => DepartmentIdentifier.Create(identifier.Value));
        RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("ParentId is not valid").When(x => x.DepartmentId != null);
        RuleFor(x => x.LocationsIds).NotEmpty().NotNull()
            .WithMessage("DepartmentsLocationsList is not valid")
            .Must(list =>
            {
                IEnumerable<LocationId> locationIds = list.ToList();
                return locationIds.Select(item => item.Value).Distinct().Count() == locationIds.Count();
            })
            .WithMessage("DepartmentsLocationsList contains duplicates");
        RuleFor(x => x.department).SetValidator(new DepartmentsValidator()!).WithMessage("Department is not valid")
            .When(x => x.department != null);
        RuleFor(x => x.Depth).NotEmpty().WithMessage("Depth is not valid").When(x => x.department != null);
    }
}