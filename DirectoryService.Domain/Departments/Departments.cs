using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class Departments
{
    public DepartmentId Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public DepartmentIdentifier Identifier { get; private set; }

    public DepartmentPath Path { get; private set; }

    public Guid? ParentId { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentsPositionsList { get; private set; } = null!;

    public IReadOnlyList<DepartmentLocation> DepartmentsLocationsList { get; private set; } = null!;

    public Departments(DepartmentId id, DepartmentName name, DepartmentIdentifier identifier, DepartmentPath path,
        Guid? parentId, short depth, bool isActive, DateTime createdAt, DateTime updatedAt,
        IReadOnlyList<DepartmentPosition> departmentsPositionsList,
        IReadOnlyList<DepartmentLocation> departmentsLocationsList)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        Path = path;
        ParentId = parentId;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DepartmentsPositionsList = departmentsPositionsList;
        DepartmentsLocationsList = departmentsLocationsList;
    }

    public static Result<Departments> Create(DepartmentId id, DepartmentName name, DepartmentIdentifier identifier,
        DepartmentPath path,
        Guid? parentId, short depth, bool isActive, DateTime createdAt, DateTime updatedAt,
        IReadOnlyList<DepartmentPosition> departmentsPositionsList,
        IReadOnlyList<DepartmentLocation> departmentsLocationsList)
    {
        Departments departments = new(id, name, identifier, path, parentId, depth, isActive, createdAt, updatedAt,
            departmentsPositionsList, departmentsLocationsList);

        return Result.Success(departments);
    }

    public void SetName(DepartmentName name) => Name = name;

    public void SetIdentifier(DepartmentIdentifier identifier) => Identifier = identifier;

    public void SetPath(DepartmentPath path) => Path = path;

    public void SetParentId(Guid? parentId) => ParentId = parentId;

    public void SetDepth(short depth) => Depth = depth;

    public void SetIsActive(bool isActive) => IsActive = isActive;

    public void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;

    public void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

    public void SetDepartmentsPositionsList(IReadOnlyList<DepartmentPosition> departmentsPositionsList) =>
        DepartmentsPositionsList = departmentsPositionsList;

    public void SetDepartmentsLocationsList(IReadOnlyList<DepartmentLocation> departmentsLocationsList) =>
        DepartmentsLocationsList = departmentsLocationsList;
}