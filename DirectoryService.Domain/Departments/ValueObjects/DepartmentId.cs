﻿namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentId
{
    public Guid Value { get; }
    private DepartmentId(Guid value)
    {
        Value = value;
    }

    public static DepartmentId NewDepartmentId() => new(Guid.NewGuid());
    public static DepartmentId Empty() => new(Guid.Empty);
    public static DepartmentId FromValue(Guid value) => new(value);
};