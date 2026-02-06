using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;
using Shared.Database;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Commands.MoveDepartments;

public class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<MoveDepartmentHandler> _logger;

    public MoveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        IValidator<MoveDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<MoveDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        MoveDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogWarning("Validation failed for DepartmentRequest: {Errors}", errors);
            return errors;
        }

        var departmentId = DepartmentId.Create(command.DepartmentId);
        var parentId = command.Request.ParentId;

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        await _departmentsRepository.LockDescendants(department.Path, cancellationToken);

        if (parentId != null)
        {
            var parentResult = await _departmentsRepository.GetByIdWithLock(
                DepartmentId.Create(parentId.Value), cancellationToken);
            if (parentResult.IsFailure)
            {
                transaction.Rollback();
                return parentResult.Error.ToErrors();
            }

            var parent = parentResult.Value;

            var checkParentIsChildResult = await _departmentsRepository.CheckParentIsChild(
                parent.Path, department.Path, cancellationToken);
            if (checkParentIsChildResult.IsFailure)
            {
                transaction.Rollback();
                return checkParentIsChildResult.Error.ToErrors();
            }

            var moveDepartmentResult = await _departmentsRepository.MoveDepartments(
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
            var moveDepartmentResult =
                await _departmentsRepository.MoveDepartments(department.Path, cancellationToken);
            if (moveDepartmentResult.IsFailure)
            {
                transaction.Rollback();
                return moveDepartmentResult.Error.ToErrors();
            }

            department.UpdateParent();
        }

        await _transactionManager.SaveChangeAsync(cancellationToken);

        var commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
        {
            transaction.Rollback();
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation("Move department {DepartmentId.Value} completed successfully.", departmentId.Value);
        return departmentId.Value;
    }
}