using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Domain.Positions;

public class Position
{
    //EF Core
    public Position() { }

    public PositionId Id { get; private set; }

    public PositionName Name { get; private set; }

    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositionsList { get; private set; }

    private Position(PositionId id, PositionName name, PositionDescription description, bool isActive,
        DateTime createdAt, DateTime updatedAt, IReadOnlyList<DepartmentPosition> departmentPositionsList)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DepartmentPositionsList = departmentPositionsList;
    }

    public static Result<Position> Create(PositionId id, PositionName name, PositionDescription description, bool isActive,
        DateTime createdAt, DateTime updatedAt, IReadOnlyList<DepartmentPosition> departmentPositionsList)
    {
        Position position = new(id, name, description, isActive, createdAt, updatedAt, departmentPositionsList);

        return Result.Success(position);
    }

    public void SetId(PositionId id) => Id = id;

    public void SetName(PositionName name) => Name = name;

    public void SetDescription(PositionDescription description) => Description = description;

    public void SetIsActive(bool isActive) => IsActive = isActive;

    public void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;

    public void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

    public void SetDepartmentPositionsList(IReadOnlyList<DepartmentPosition> departmentPositionsList) =>
        DepartmentPositionsList = departmentPositionsList;
}