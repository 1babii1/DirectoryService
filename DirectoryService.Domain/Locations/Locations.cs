using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Domain.Locations;

public class Locations
{
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }

    public Timezone Timezone { get; private set; }

    public Address Address { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocationsList { get; private set; }

    private Locations(LocationId id, LocationName name, Timezone timezone, Address address, bool isActive,
        DateTime createdAt, DateTime updatedAt, IReadOnlyList<DepartmentLocation> departmentLocationsList)
    {
        Id = id;
        Name = name;
        Timezone = timezone;
        Address = address;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DepartmentLocationsList = departmentLocationsList;
    }

    public Result<Locations> Create(
        LocationId id,
        LocationName name,
        Timezone timezone,
        Address address,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        IReadOnlyList<DepartmentLocation> departmentLocationsList)
    {
        Locations locations = new(id, name, timezone, address, isActive, createdAt, updatedAt, departmentLocationsList);

        return Result.Success(locations);
    }

    public void SetId(LocationId id) => Id = id;

    public void SetName(LocationName name) => Name = name;

    public void SetTimezone(Timezone timezone) => Timezone = timezone;

    public void SetAddress(Address address) => Address = address;

    public void SetIsActive(bool isActive) => IsActive = isActive;

    public void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;


    public void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

    public void SetDepartmentLocationsList(IReadOnlyList<DepartmentLocation> departmentLocationsList) =>
        DepartmentLocationsList = departmentLocationsList;
}