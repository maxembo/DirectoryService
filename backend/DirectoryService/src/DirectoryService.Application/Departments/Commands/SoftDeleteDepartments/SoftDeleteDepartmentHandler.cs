using CSharpFunctionalExtensions;
using DirectoryService.Application.Constants;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;

public class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        IValidator<SoftDeleteDepartmentCommand> validator,
        HybridCache cache,
        ILogger<SoftDeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var departmentId = DepartmentId.Create(command.DepartmentId);

        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);
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

        Result<Department, Error> departmentResult = await _departmentsRepository.GetBy(
            d => d.Id == departmentId && d.IsActive == true, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transaction.Rollback();
            return departmentResult.Error.ToErrors();
        }

        Department? department = departmentResult.Value;

        department.MarkAsDelete();

        UnitResult<Error> updatePathsMarkDeleteResult =
            await _departmentsRepository.UpdatePathsMarkDelete(department.Path, cancellationToken);
        if (updatePathsMarkDeleteResult.IsFailure)
        {
            transaction.Rollback();
            return updatePathsMarkDeleteResult.Error.ToErrors();
        }

        UnitResult<Error> deleteUnusedLocationsResult =
            await _locationsRepository.SoftDeleteUnusedLocationsByDepartmentIdAsync(departmentId, cancellationToken);
        if (deleteUnusedLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteUnusedLocationsResult.Error.ToErrors();
        }

        UnitResult<Error> deleteUnusedPositionsResult =
            await _positionsRepository.SoftDeleteUnusedPositionsByDepartmentIdAsync(departmentId, cancellationToken);
        if (deleteUnusedPositionsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteUnusedPositionsResult.Error.ToErrors();
        }

        UnitResult<Error> saveChangesResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transaction.Rollback();
            return saveChangesResult.Error.ToErrors();
        }

        UnitResult<Error> commitResult = transaction.Commit();
        if (commitResult.IsFailure)
        {
            transaction.Rollback();
            return commitResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, cancellationToken);

        _logger.LogInformation("Department {DepartmentId} soft deleted successfully.", department.Id.Value);

        return departmentId.Value;
    }
}