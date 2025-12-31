using CSharpFunctionalExtensions;
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

public class SoftDeleteDepartmentHandler
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly SoftDeleteDepartmentValidation _validation;

    public SoftDeleteDepartmentHandler(
        IDepartmentRepository departmentRepository, ILocationsRepository locationsRepository,
        IPositionRepository positionRepository,
        ITransactionManager transactionManager, ILogger<SoftDeleteDepartmentHandler> logger,
        SoftDeleteDepartmentValidation validation)
    {
        _departmentRepository = departmentRepository;
        _locationsRepository = locationsRepository;
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validation = validation;
    }

    public async Task<Result<DepartmentId, Error>> Handle(
        SoftDeleteDepartmentRequest request,
        CancellationToken cancellationToken)
    {
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

        // Мягкое удаление департамента
        department.Value.Delete();

        // Получение осиротевших Локации
        var locationOrphan =
            await _locationsRepository.GetOrphanLocationByDepartment(department.Value.Id, cancellationToken);
        if (locationOrphan.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("fail to get orphan locations");
            return locationOrphan.Error;
        }

        if (locationOrphan.Value.Any())
        {
            foreach (var location in locationOrphan.Value)
            {
                location.Delete();
            }
        }

        // Получение осиротевших Локации
        var positionOrphan =
            await _positionRepository.GetOrphanPositionByDepartment(department.Value.Id, cancellationToken);
        if (positionOrphan.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("fail to get orphan locations");
            return positionOrphan.Error;
        }

        if (positionOrphan.Value.Any())
        {
            foreach (var position in positionOrphan.Value)
            {
                position.Delete();
            }
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        transactionScope.Commit();

        _logger.LogInformation(
            "Департамент удален{0}{1}",
            locationOrphan.Value?.Any() == true ? ", удалены связанные локации" : " ",
            positionOrphan.Value?.Any() == true ? ", удалены связанные позиции" : " ");

        return department.Value.Id;
    }
}