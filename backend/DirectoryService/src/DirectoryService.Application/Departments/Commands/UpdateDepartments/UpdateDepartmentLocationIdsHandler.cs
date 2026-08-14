using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments.UpdateDepartments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartments;

public class UpdateDepartmentLocationIdsHandler : ICommandHandler<Guid, UpdateDepartmentLocationIdsCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateDepartmentLocationIdsHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentLocationIdsRequest> _validator;

    public UpdateDepartmentLocationIdsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationIdsRequest> validator,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentLocationIdsHandler> logger, HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationIdsCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var transactionResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        var departmentId = DepartmentId.Create(command.DepartmentId);

        var getDepartmentResult =
            await _departmentsRepository.GetByIdAsync(departmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            transaction.Rollback();
            return getDepartmentResult.Error.ToErrors();
        }

        var department = getDepartmentResult.Value;

        var checkExistingIdsResult =
            await _locationsRepository.CheckExistingAndActiveIdsAsync(command.Request.LocationIds, cancellationToken);
        if (checkExistingIdsResult.IsFailure)
        {
            transaction.Rollback();
            return checkExistingIdsResult.Error;
        }

        var locationIds = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(
                DepartmentLocationId.CreateNew(), department.Id, LocationId.Create(locationId)));

        var updateLocationIdsResult = department.UpdateLocationIds(locationIds);
        if (updateLocationIdsResult.IsFailure)
        {
            transaction.Rollback();
            return updateLocationIdsResult.Error.ToErrors();
        }

        var deleteLocationsResult =
            await _departmentsRepository.DeleteLocationsAsync(departmentId, cancellationToken);
        if (deleteLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteLocationsResult.Error.ToErrors();
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

        _logger.LogInformation("Department {Department.Id} location ids updated.", department.Id.Value);

        return department.Id.Value;
    }
}