using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department.Commands;

public class SoftDeleteDepartmentValidation : AbstractValidator<SoftDeleteDepartmentRequest>
{
    public SoftDeleteDepartmentValidation()
    {
        RuleFor(x => x.departmentId).NotNull().NotEmpty().WithMessage("DepartmentId should not be empty");
    }
}

public class SoftDeleteDepartmentHandle
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandle> _logger;
    private readonly SoftDeleteDepartmentValidation _validation;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public SoftDeleteDepartmentHandle(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager, ILogger<SoftDeleteDepartmentHandle> logger,
        SoftDeleteDepartmentValidation validation, IDbConnectionFactory dbConnectionFactory)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validation = validation;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<DepartmentId, Error>> Handle(
        SoftDeleteDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
        {
            return transaction.Error;
        }

        using var transactionScope = transaction.Value;

        // Валидация данных
        var validateResult = await _validation.ValidateAsync(request, cancellationToken);
        if (validateResult.IsValid == false)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to validate update department locations");
            return validateResult.ToError();
        }

        // Проверка на существование Департамента
        var department =
            await _departmentRepository.GetById(DepartmentId.FromValue(request.departmentId), cancellationToken);
        if (department.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to get department");
            return department.Error;
        }

        if (department.Value.IsActive == false)
        {
            transactionScope.Rollback();
            _logger.LogError("department deleted");
            return department.Error;
        }

        var deletedDepartment = await _departmentRepository.Delete(department.Value, cancellationToken);
        if (deletedDepartment.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("department not deleted");
            return department.Error;
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        transactionScope.Commit();

        return deletedDepartment;
    }
}