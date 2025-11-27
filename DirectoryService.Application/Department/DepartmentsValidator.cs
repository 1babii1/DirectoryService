using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Department;

public class DepartmentsValidator : AbstractValidator<Departments>
{
    public DepartmentsValidator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull().WithMessage("Id is not valid");
        RuleFor(x => x.Name).MustBeValueObject(name => DepartmentName.Create(name.Value));
        RuleFor(x => x.Identifier).MustBeValueObject(identifier => DepartmentIdentifier.Create(identifier.Value));
        RuleFor(x => x.ParentId).NotEmpty().WithMessage("ParentId is not valid").When(x => x.ParentId != null);
        RuleFor(x => x.Path).NotEmpty().NotNull().WithMessage("Path is not valid");
        RuleFor(x => x.Depth).NotEmpty().NotNull().WithMessage("Depth is not valid");
    }
}