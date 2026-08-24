using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.ChangeDepartmentActivity;

public class ChangeDepartmentActivityHandler : ICommandHandler<Guid, ChangeDepartmentActivityCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly HybridCache _cache;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ChangeDepartmentActivityHandler> _logger;

    public ChangeDepartmentActivityHandler(
        IDepartmentsRepository departmentsRepository,
        HybridCache cache,
        ITransactionManager transactionManager,
        ILogger<ChangeDepartmentActivityHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _cache = cache;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        ChangeDepartmentActivityCommand command, CancellationToken cancellationToken)
    {
        var departmentId = DepartmentId.Create(command.DepartmentId);

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transaction.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        if (department.DeletedAt != null)
        {
            transaction.Rollback();
            return DepartmentErrors.ArchivedActivityCannotBeChanged().ToErrors();
        }

        if (!command.IsActive && department.IsActive &&
            await _departmentsRepository.HasActiveDescendants(department.Path, cancellationToken))
        {
            transaction.Rollback();
            return DepartmentErrors.ActiveDescendantsPreventDeactivation().ToErrors();
        }

        if (command.IsActive && department is { IsActive: false, ParentId: not null })
        {
            var parentResult = await _departmentsRepository.GetByIdWithLock(department.ParentId, cancellationToken);
            if (parentResult.IsFailure)
            {
                transaction.Rollback();
                return DepartmentErrors.InactiveParentPreventsActivation().ToErrors();
            }

            var parent = parentResult.Value;
            if (!parent.IsActive || parent.DeletedAt is not null)
            {
                transaction.Rollback();
                return DepartmentErrors.InactiveParentPreventsActivation().ToErrors();
            }
        }

        if (department.IsActive == command.IsActive)
        {
            return department.Id.Value;
        }

        department.SetActivity(command.IsActive);

        var saveChangeResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            transaction.Rollback();
            return saveChangeResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
        {
            transaction.Rollback();
            return commitResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Department {DepartmentId} change status active successfully.", department.Id.Value);

        return department.Id.Value;
    }
}
