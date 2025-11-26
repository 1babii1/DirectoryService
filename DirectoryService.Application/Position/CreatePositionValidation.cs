using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Position;

public class CreatePositionValidation : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidation()
    {
        RuleFor(x => x.request.Name).MustBeValueObject(name => PositionName.Create(name.Value));
        RuleFor(x => x.request.Description)
            .MustBeValueObject(description => PositionDescription.Create(description!.Value))
            .When(x => x.request.Description != null);
        RuleFor(x => x.request.DepartmentIds).NotEmpty().NotNull().WithMessage("DepartmentIds is not valid")
            .Must(list =>
            {
                IEnumerable<DepartmentId> departmentIds = list.ToList();
                return departmentIds.Select(item => item.Value).Distinct().Count() == departmentIds.Count();
            }).WithMessage("DepartmentIds contains duplicates");
    }
}