using DirectoryService.Contracts.Department;
using FluentValidation;

namespace DirectoryService.Application.Department;

public class CreateDepartmentValidation : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidation()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull();
        RuleFor(x => x.Identifier).NotEmpty().NotNull();
        RuleFor(x => x.department).NotNull().SetValidator(new DepartmentsValidator()!)
            .When(x => x.department is not null);
        RuleFor(x => x.Depth).GreaterThanOrEqualTo((short)0)
            .When(x => x.Depth.HasValue);
        RuleFor(x => x.DepartmentsLocations).NotEmpty().NotNull();
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}