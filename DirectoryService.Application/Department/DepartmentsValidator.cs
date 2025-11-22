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
        RuleFor(x => x.Depth).GreaterThanOrEqualTo((short)0).WithMessage("Depth is not valid");
        RuleFor(x => x.Name).MustBeValueObject(name => DepartmentName.Create(name.ToString()));
        RuleFor(x => x.Identifier)
            .MustBeValueObject(identidifier => DepartmentIdentifier.Create(identidifier.ToString()));
        RuleFor(x => x.Path).MustBeValueObject(path => DepartmentPath.CreateParent(path.ToString()));
        RuleFor(x => x.ParentId).NotEmpty().WithMessage("ParentId is not valid").When(x => x.ParentId != null);

        RuleFor(x => x.DepartmentsChildrenList)
            .NotNull()
            .ForEach(child => child.SetValidator(new DepartmentsValidator()));

        RuleFor(x => x.DepartmentsPositionsList)
            .NotNull()
            .NotEmpty().WithMessage("DepartmentPositionList is not valid");

        RuleFor(x => x.DepartmentsLocationsList)
            .NotNull()
            .NotEmpty().WithMessage("DepartmentsLocationsList is not valid");

        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}