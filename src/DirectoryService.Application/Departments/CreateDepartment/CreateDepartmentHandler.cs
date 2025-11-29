using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreateDepartmentRequest> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentRequest> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();

            _logger.LogWarning("Validation failed for LocationRequest: {Errors}", errors);
            return validationResult.ToErrors();
        }

        var departmentId = DepartmentId.CreateNew();
        var name = DepartmentName.Create(command.Request.Name).Value;
        var identifier = Identifier.Create(command.Request.Identifier).Value;

        var parentId = command.Request.ParentId;

        var checkExistingIdsResult = await _locationsRepository.CheckExistingAndActiveIdsAsync(
            command.Request.LocationsIds, cancellationToken);
        if (checkExistingIdsResult.IsFailure)
            return checkExistingIdsResult.Error;

        var locationIds =
            command.Request.LocationsIds.Select(
                    l => new DepartmentLocation(DepartmentLocationId.CreateNew(), departmentId, LocationId.Create(l)))
                .ToList();

        Department department;
        if (parentId == null)
        {
            var createParentDepartmentResult = Department.CreateParent(name, identifier, locationIds);
            if (createParentDepartmentResult.IsFailure)
                return createParentDepartmentResult.Error.ToErrors();

            department = createParentDepartmentResult.Value;
        }
        else
        {
            var getParentDepartmentResult =
                await _departmentsRepository.GetByIdAsync(parentId.Value, cancellationToken);
            if (getParentDepartmentResult.IsFailure)
                return getParentDepartmentResult.Error.ToErrors();

            var childParentDepartmentResult = Department.CreateChild(
                name, identifier, getParentDepartmentResult.Value, locationIds, departmentId);
            if (childParentDepartmentResult.IsFailure)
                return childParentDepartmentResult.Error.ToErrors();

            department = childParentDepartmentResult.Value;
        }

        var repositoryResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (repositoryResult.IsFailure)
        {
            return Error.Failure(null, repositoryResult.Error.Message)
                .ToErrors();
        }

        _logger.LogInformation("Department {DepartmentId.Value} has been created.", departmentId.Value);

        return departmentId.Value;
    }
}