using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public partial record DepartmentPath
{
    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(null, "Department path is required");

        string trimmed = value.Trim().Replace(" ", "-");

        if (!LatinDotHyphenRegex().IsMatch(trimmed))
        {
            return Error.Validation(null, "Department path is invalid");
        }

        DepartmentPath result = new(trimmed);

        return Result.Success<DepartmentPath, Error>(result);
    }

    [GeneratedRegex(@"^[a-zA-Z.-]+$")]
    private static partial Regex LatinDotHyphenRegex();
}