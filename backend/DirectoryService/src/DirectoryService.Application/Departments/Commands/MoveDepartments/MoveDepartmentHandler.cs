using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Domain.Departments;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.MoveDepartments;

public class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<MoveDepartmentHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<MoveDepartmentCommand> _validator;

    public MoveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        IValidator<MoveDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<MoveDepartmentHandler> logger,
        HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(
        MoveDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogWarning("Validation failed for DepartmentRequest: {Errors}", errors);
            return errors;
        }

        var departmentId = DepartmentId.Create(command.DepartmentId);
        Guid? parentId = command.Request.ParentId;

        Result<ITransactionScope, Error> transactionResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using ITransactionScope? transaction = transactionResult.Value;

        Result<Department, Error> departmentResult =
            await _departmentsRepository.GetActiveByIdWithLock(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        Department? department = departmentResult.Value;

        await _departmentsRepository.LockDescendants(department.Path, cancellationToken);

        if (parentId != null)
        {
            Result<Department, Error> parentResult = await _departmentsRepository.GetActiveByIdWithLock(
                DepartmentId.Create(parentId.Value), cancellationToken);
            if (parentResult.IsFailure)
            {
                transaction.Rollback();
                return parentResult.Error.ToErrors();
            }

            Department? parent = parentResult.Value;

            UnitResult<Error> checkParentIsChildResult = await _departmentsRepository.CheckParentIsChild(
                parent.Path, department.Path, cancellationToken);
            if (checkParentIsChildResult.IsFailure)
            {
                transaction.Rollback();
                return checkParentIsChildResult.Error.ToErrors();
            }

            UnitResult<Error> moveDepartmentResult = await _departmentsRepository.MoveDepartments(
                DepartmentId.Create(parentId.Value),
                parent.Path, department.Path, cancellationToken);
            if (moveDepartmentResult.IsFailure)
            {
                transaction.Rollback();
                return moveDepartmentResult.Error.ToErrors();
            }

            department.UpdateParent(DepartmentId.Create(parentId.Value));
        }
        else
        {
            UnitResult<Error> moveDepartmentResult =
                await _departmentsRepository.MoveDepartments(department.Path, cancellationToken);
            if (moveDepartmentResult.IsFailure)
            {
                transaction.Rollback();
                return moveDepartmentResult.Error.ToErrors();
            }

            department.UpdateParent();
        }

        await _transactionManager.SaveChangeAsync(cancellationToken);

        UnitResult<Error> commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
        {
            transaction.Rollback();
            return commitedResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Move department {DepartmentId.Value} completed successfully.", departmentId.Value);

        return departmentId.Value;
    }
}