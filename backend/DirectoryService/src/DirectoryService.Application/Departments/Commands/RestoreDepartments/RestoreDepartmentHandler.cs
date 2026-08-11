using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Application.Departments.Commands.RestoreDepartments;

public class RestoreDepartmentHandler : ICommandHandler<Guid, RestoreDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;
    private readonly ILogger<RestoreDepartmentHandler> _logger;

    public RestoreDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        HybridCache cache,
        ILogger<RestoreDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        RestoreDepartmentCommand command, CancellationToken cancellationToken)
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

        if (department.IsActive)
        {
            transaction.Rollback();
            _logger.LogInformation(
                "Department {DepartmentId} is already active.",
                department.Id.Value);
            return department.Id.Value;
        }

        await _departmentsRepository.LockDescendants(department.Path, cancellationToken);

        Path restoredPath;

        if (department.ParentId != null)
        {
            var parentDepartmentResult
                = await _departmentsRepository.GetByIdWithLock(department.ParentId, cancellationToken);
            if (parentDepartmentResult.IsFailure)
            {
                transaction.Rollback();
                return parentDepartmentResult.Error.ToErrors();
            }

            var parentDepartment = parentDepartmentResult.Value;

            if (!parentDepartment.IsActive)
            {
                transaction.Rollback();
                return DepartmentErrors.ParentIsArchived().ToErrors();
            }

            restoredPath = parentDepartment.Path.CreateChild(department.Identifier);
        }
        else
        {
            restoredPath = Path.CreateParent(department.Identifier);
        }

        var restorePathsResult = await _departmentsRepository.RestoreSubtreePaths(
            department.Path, restoredPath, cancellationToken);
        if (restorePathsResult.IsFailure)
        {
            transaction.Rollback();
            return restorePathsResult.Error.ToErrors();
        }

        department.Restore();

        var restoreLocationsResult =
            await _locationsRepository.RestoreLocationsByDepartmentIdAsync(departmentId, cancellationToken);
        if (restoreLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return restoreLocationsResult.Error.ToErrors();
        }

        var restorePositionsResult =
            await _positionsRepository.RestorePositionsByDepartmentIdAsync(departmentId, cancellationToken);
        if (restorePositionsResult.IsFailure)
        {
            transaction.Rollback();
            return restorePositionsResult.Error.ToErrors();
        }

        var saveChangeResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            transaction.Rollback();
            return saveChangeResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Department {DepartmentId} restored successfully.", department.Id.Value);

        return department.Id.Value;
    }
}