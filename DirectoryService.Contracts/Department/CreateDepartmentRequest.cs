using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;

namespace DirectoryService.Contracts.Department;

public record CreateDepartmentRequest(
    DepartmentName Name,
    DepartmentIdentifier Identifier,
    Departments? department,
    short? Depth,
    IEnumerable<DepartmentLocation> DepartmentsLocations,
    DepartmentId? DepartmentId);