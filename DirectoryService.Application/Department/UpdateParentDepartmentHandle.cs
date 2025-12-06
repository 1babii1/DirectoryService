using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Department;
using DirectoryService.Contracts.Response.Department;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department
{
    public record UpdateParentDepartmentCommand(Guid DepartmentId, UpdateParentDepartmentRequest Request);

    public class UpdateParentDepartmentValidation : AbstractValidator<UpdateParentDepartmentCommand>
    {
        public UpdateParentDepartmentValidation()
        {
            RuleFor(x => x.DepartmentId).NotNull().NotEmpty().WithMessage("departmentId cannot be empty");
            RuleFor(x => x.Request.parentDepartmentId).NotNull().NotEmpty().WithMessage("parentId cannot be empty");
        }
    }

    public class UpdateParentDepartmentHandle
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly UpdateParentDepartmentValidation _validator;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<UpdateParentDepartmentHandle> _logger;

        public UpdateParentDepartmentHandle(
            IDepartmentRepository departmentRepository,
            UpdateParentDepartmentValidation validator,
            ITransactionManager transaction,
            ILogger<UpdateParentDepartmentHandle> logger)
        {
            _departmentRepository = departmentRepository;
            _validator = validator;
            _transactionManager = transaction;
            _logger = logger;
        }

        public async Task<Result<DepartmentId, Error>> Handle(
            UpdateParentDepartmentCommand requestCommand,
            CancellationToken cancellationToken)
        {
            var transactionScopeResult =
                await _transactionManager.BeginTransactionAsync(cancellationToken);

            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error;
            }

            using var transactionScope = transactionScopeResult.Value;

            try
            {
                // Валидация входных данных
                ValidationResult validateResult = await _validator.ValidateAsync(requestCommand);
                if (!validateResult.IsValid)
                {
                    transactionScope.Rollback();
                    _logger.LogError("Failed to validate department");
                    return validateResult.ToError();
                }

                var newParentDepId = DepartmentId.FromValue(requestCommand.Request.parentDepartmentId);
                var currentDepId = DepartmentId.FromValue(requestCommand.DepartmentId);

                // Блокировка того с чем будем работать
                var newParentDep = await _departmentRepository.GetByIdWithLock(newParentDepId, cancellationToken);
                if (newParentDep.IsFailure)
                {
                    transactionScope.Rollback();
                    _logger.LogError("could not be found");
                    return newParentDep.Error;
                }

                var currentDep = await _departmentRepository.GetByIdWithLock(currentDepId, cancellationToken);
                if (currentDep.IsFailure)
                {
                    transactionScope.Rollback();
                    _logger.LogError("could not be found");
                    return currentDep.Error;
                }

                var lockChildren =
                    await _departmentRepository.LockChildrenByPath(newParentDep.Value.Path, cancellationToken);
                if (lockChildren.IsFailure)
                {
                    transactionScope.Rollback();
                    _logger.LogError("could not be found");
                    return currentDep.Error;
                }

                _logger.LogInformation(
                    "newParentDepList department: {newParentDep}",
                    JsonSerializer.Serialize(newParentDep.Value, new JsonSerializerOptions { WriteIndented = true }));

                _logger.LogInformation(
                    "currentDep department: {currentDep}",
                    JsonSerializer.Serialize(currentDep.Value, new JsonSerializerOptions { WriteIndented = true }));

                // Проверка id
                var idChild = await _departmentRepository.GetChildrenIdsAsync(currentDep.Value.Path, cancellationToken);

                if (idChild.Contains(currentDepId.Value))
                {
                    transactionScope.Rollback();
                    _logger.LogError("You cannot select yourself or your child department");
                    return Error.Conflict("department.id", "cannot select yourself or your child department");
                }

                var newDepth = (short)(currentDep.Value.Depth - newParentDep.Value.Depth - 1);

                _logger.LogInformation(
                    $"{newParentDepId}, {newParentDep.Value.Path}, {currentDep.Value.Path}, {newDepth}");

                var updateParent = await _departmentRepository.UpdateHierarchy(newParentDepId, newParentDep.Value.Path,
                    currentDep.Value.Path, newDepth, cancellationToken);

                var save = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (save.IsFailure)
                {
                    transactionScope.Rollback();
                    _logger.LogError("failed");
                    return save.Error;
                }

                transactionScope.Commit();

                return newParentDepId;
            }
            catch (Exception)
            {
                transactionScope.Rollback();
                throw;
            }
        }
    }
}