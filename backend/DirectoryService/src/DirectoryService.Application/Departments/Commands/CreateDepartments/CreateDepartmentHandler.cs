using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments.CreateDepartments;
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

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<CreateDepartmentRequest> _validator;

    public CreateDepartmentHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentRequest> validator,
        ILogger<CreateDepartmentHandler> logger,
        HybridCache cache,
        ITransactionManager transactionManager)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _cache = cache;
        _transactionManager = transactionManager;
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
            command.Request.LocationIds, cancellationToken);
        if (checkExistingIdsResult.IsFailure)
        {
            return checkExistingIdsResult.Error;
        }

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        var locationIds =
            command.Request.LocationIds.Select(l => new DepartmentLocation(
                    DepartmentLocationId.CreateNew(), departmentId, LocationId.Create(l)))
                .ToList();

        Department department;
        if (parentId == null)
        {
            var createParentDepartmentResult =
                Department.CreateParent(name, identifier, locationIds, departmentId);
            if (createParentDepartmentResult.IsFailure)
            {
                return createParentDepartmentResult.Error.ToErrors();
            }

            department = createParentDepartmentResult.Value;
        }
        else
        {
            var parentDepartmentId = DepartmentId.Create(parentId.Value);

            var getParentDepartmentResult =
                await _departmentsRepository.GetByIdWithLock(parentDepartmentId, cancellationToken);
            if (getParentDepartmentResult.IsFailure)
            {
                transaction.Rollback();
                return getParentDepartmentResult.Error.ToErrors();
            }

            if (!getParentDepartmentResult.Value.IsActive || getParentDepartmentResult.Value.DeletedAt is not null)
            {
                transaction.Rollback();
                return GeneralErrors.NotFound("department", parentDepartmentId.Value).ToErrors();
            }

            var childParentDepartmentResult = Department.CreateChild(
                name, identifier, getParentDepartmentResult.Value, locationIds, departmentId);
            if (childParentDepartmentResult.IsFailure)
            {
                return childParentDepartmentResult.Error.ToErrors();
            }

            department = childParentDepartmentResult.Value;
        }

        var repositoryResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (repositoryResult.IsFailure)
        {
            transaction.Rollback();
            return Error.Failure(null, repositoryResult.Error.Message)
                .ToErrors();
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
        {
            transaction.Rollback();
            return commitResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Department {DepartmentId.Value} has been created.", departmentId.Value);

        return departmentId.Value;
    }
}
