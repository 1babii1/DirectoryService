namespace DirectoryService.Domain.DepartmentPositions.ValueObjects;

public record DepartmentPositionsId
{
    public Guid Value { get; }

    // EF Core
    public DepartmentPositionsId() { }

    private DepartmentPositionsId(Guid value)
    {
        Value = value;
    }

    public static DepartmentPositionsId NewDepartmentPositionsId() => new(Guid.NewGuid());

    public static DepartmentPositionsId FromValue(Guid value) => new(value);

    public static DepartmentPositionsId Empty() => new(Guid.Empty);
}