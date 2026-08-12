using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using FluentValidation.Results;
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
        ValidationResult? validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        Result<ITransactionScope, Error> transactionResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using ITransactionScope? transaction = transactionResult.Value;

        var departmentId = DepartmentId.Create(command.DepartmentId);

        Result<Department, Error> getDepartmentResult =
            await _departmentsRepository.GetByIdAsync(departmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            transaction.Rollback();
            return getDepartmentResult.Error.ToErrors();
        }

        Department? department = getDepartmentResult.Value;

        UnitResult<Errors> checkExistingIdsResult =
            await _locationsRepository.CheckExistingAndActiveIdsAsync(command.Request.LocationIds, cancellationToken);
        if (checkExistingIdsResult.IsFailure)
        {
            transaction.Rollback();
            return checkExistingIdsResult.Error;
        }

        IEnumerable<DepartmentLocation>? locationIds = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(
                DepartmentLocationId.CreateNew(), department.Id, LocationId.Create(locationId)));

        department.UpdateLocationIds(locationIds);

        await _departmentsRepository.DeleteLocationsAsync(departmentId, cancellationToken);

        await _transactionManager.SaveChangeAsync(cancellationToken);

        UnitResult<Error> commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Department {Department.Id} location ids updated.", department.Id.Value);

        return department.Id.Value;
    }
}