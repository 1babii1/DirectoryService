namespace DirectoryService.Domain.DepartmentLocations.ValueObjects;

public record DepartmentLocationsId
{
    public Guid Value { get; }

    private DepartmentLocationsId(Guid value)
    {
        Value = value;
    }

    public static DepartmentLocationsId NewDepartmentLocationsId() => new(Guid.NewGuid());
    public static DepartmentLocationsId Empty() => new(Guid.Empty);
    public static DepartmentLocationsId FromValue(Guid value) => new(value);
}