using DirectoryService.Contracts.Department;
using FluentValidation;

namespace DirectoryService.Application.Department;

public class CreateDepartmentValidation : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidation()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull().WithMessage("Name is not valid");
        RuleFor(x => x.Identifier).NotEmpty().NotNull().WithMessage("Identifier is not valid");
        RuleFor(x => x.department).NotNull().SetValidator(new DepartmentsValidator()!)
            .WithMessage("Department is not valid")
            .When(x => x.department is not null);
        RuleFor(x => x.Depth).GreaterThanOrEqualTo((short)0)
            .WithMessage("Depth is not valid")
            .When(x => x.Depth.HasValue);
        RuleFor(x => x.DepartmentsLocations).NotEmpty().NotNull().WithMessage("DepartmentsLocations is not valid");
        RuleFor(x => x.DepartmentId).NotEmpty()
            .WithMessage("DepartmentId is not valid")
            .When(x => x.DepartmentId is not null);
    }
}