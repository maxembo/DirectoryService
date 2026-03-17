using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public class DeleteDepartmentsService : IDeleteDepartmentsService
{
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILocationsRepository _locationsRepository;

    public DeleteDepartmentsService(
        ITransactionManager transactionManager,
        IDepartmentsRepository departmentsRepository,
        IPositionsRepository positionsRepository,
        ILocationsRepository locationsRepository)
    {
        _transactionManager = transactionManager;
        _departmentsRepository = departmentsRepository;
        _positionsRepository = positionsRepository;
        _locationsRepository = locationsRepository;
    }

    public async Task<UnitResult<Error>> Process(CancellationToken cancellationToken = default)
    {
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        using var transaction = transactionResult.Value;

        var deleteDepartmentLocationsResult =
            await _departmentsRepository.DeleteDepartmentLocationsMarkDelete(cancellationToken);
        if (deleteDepartmentLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteDepartmentLocationsResult.Error;
        }

        var deleteDepartmentPositionsResult =
            await _departmentsRepository.DeleteDepartmentPositionsMarkDelete(cancellationToken);
        if (deleteDepartmentPositionsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteDepartmentPositionsResult.Error;
        }

        var updatePathsDeleteResult = await _departmentsRepository.UpdatePathsAfterDelete(cancellationToken);
        if (updatePathsDeleteResult.IsFailure)
        {
            transaction.Rollback();
            return updatePathsDeleteResult.Error;
        }

        var deleteDepartmentsResult = await _departmentsRepository.DeleteDepartmentsMarkDelete(cancellationToken);
        if (deleteDepartmentsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteDepartmentsResult.Error;
        }

        var deleteLocationsResult = await _locationsRepository.DeleteLocationsMarkDelete(cancellationToken);
        if (deleteLocationsResult.IsFailure)
        {
            transaction.Rollback();
            return deleteLocationsResult.Error;
        }

        var deletePositionsResult = await _positionsRepository.DeletePositionsMarkDelete(cancellationToken);
        if (deletePositionsResult.IsFailure)
        {
            transaction.Rollback();
            return deletePositionsResult.Error;
        }

        var commitedResult = transaction.Commit();
        if (commitedResult.IsFailure)
        {
            transaction.Rollback();
            return commitedResult.Error;
        }

        return UnitResult.Success<Error>();
    }
}