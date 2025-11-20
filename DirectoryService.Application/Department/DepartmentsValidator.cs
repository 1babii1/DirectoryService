using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Application.Department;

public class DepartmentsValidator : AbstractValidator<Departments>
{
    public DepartmentsValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Depth).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.Name).NotEmpty().NotNull();
        RuleFor(x => x.Identifier).NotEmpty().NotNull();
        RuleFor(x => x.Path).NotEmpty().NotNull();
        RuleFor(x => x.DepartmentsLocationsList).NotEmpty().NotNull();
    }
}