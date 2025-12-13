using DirectoryService.Domain.Departments.ValueObjects;

namespace DirectoryService.Contracts.Department;

public record UpdateParentDepartmentRequest(Guid parentDepartmentId);