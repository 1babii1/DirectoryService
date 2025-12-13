using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Contracts.Department;

public record CreateDepartmentRequest(
    DepartmentName Name,
    DepartmentIdentifier Identifier,
    DepartmentId? ParentDepartmentId,
    short? Depth,
    IEnumerable<LocationId> LocationsIds,
    DepartmentId? DepartmentId);