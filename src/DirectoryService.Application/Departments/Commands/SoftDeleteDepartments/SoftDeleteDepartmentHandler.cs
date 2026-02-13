using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Shared;
using Shared.Abstractions;
using Shared.Database;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;

public class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        IValidator<SoftDeleteDepartmentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var departmentId = DepartmentId.Create(command.DepartmentId);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        var departmentResult = await _departmentsRepository.GetBy(
            d => d.Id == departmentId && d.IsActive == true, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transaction.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        department.MarkAsDelete();

        var updatePathsMarkDeleteResult =
            await _departmentsRepository.UpdatePathsMarkDelete(department.Path, cancellationToken);
        if (updatePathsMarkDeleteResult.IsFailure)
        {
            transaction.Rollback();
            return updatePathsMarkDeleteResult.Error.ToErrors();
        }

        var deleteUnusedLocationsResult =
            await _locationsRepository.DeleteUnusedLocationsByDepartmentIdAsync(departmentId, cancellationToken);
        if (deleteUnusedLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteUnusedLocationsResult.Error.ToErrors();
        }

        var deleteUnusedPositionsResult =
            await _positionsRepository.DeleteUnusedPositionsByDepartmentIdAsync(departmentId, cancellationToken);
        if (deleteUnusedPositionsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteUnusedPositionsResult.Error.ToErrors();
        }

        await _transactionManager.SaveChangeAsync(cancellationToken);

        var commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
        {
            transaction.Rollback();
            return commitedResult.Error.ToErrors();
        }

        return departmentId.Value;
    }
}