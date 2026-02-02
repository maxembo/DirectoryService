using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartments;

public class UpdateDepartmentLocationIdsHandler : ICommandHandler<Guid, UpdateDepartmentLocationIdsCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateDepartmentLocationIdsRequest> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentLocationIdsHandler> _logger;

    public UpdateDepartmentLocationIdsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationIdsRequest> validator,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentLocationIdsHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationIdsCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var departmentId = DepartmentId.Create(command.DepartmentId);

        var getDepartmentResult = await _departmentsRepository.GetByIdAsync(departmentId, cancellationToken);
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
            .Select(
                locationId => new DepartmentLocation(
                    DepartmentLocationId.CreateNew(), department.Id, LocationId.Create(locationId)));

        department.UpdateLocationIds(locationIds);

        await _departmentsRepository.DeleteLocationsAsync(departmentId, cancellationToken);

        await _transactionManager.SaveChangeAsync(cancellationToken);

        var commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        _logger.LogInformation("Department {Department.Id} location ids updated.", department.Id);

        return department.Id.Value;
    }
}