using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Postgres.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(DirectoryServiceDbContext dbContext, ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(location, cancellationToken);

        UnitResult<Error> saveChangesResult = await _dbContext.SaveChangesResultAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        return location.Id.Value;
    }

    public async Task<Result<Location, Error>> GetByIdAsync(
        LocationId locationId, CancellationToken cancellationToken = default)
    {
        Location? location = await _dbContext.Locations
            .Where(l => l.IsActive == true)
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location == null)
        {
            return GeneralErrors.NotFound("location", locationId.Value);
        }

        return location;
    }

    public async Task<Result<Location, Error>> GetByIdIncludingInactiveAsync(
        LocationId locationId, CancellationToken cancellationToken = default)
    {
        Location? location = await _dbContext.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location is null)
        {
            return GeneralErrors.NotFound("location", locationId.Value);
        }

        return location;
    }

    public async Task<UnitResult<Errors>> CheckExistingAndActiveIdsAsync(
        Guid[] ids, CancellationToken cancellationToken = default)
    {
        LocationId[]? locationIds = LocationId.Create(ids);

        List<Guid>? existingIds = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id) && l.IsActive == true)
            .Select(l => l.Id.Value)
            .ToListAsync(cancellationToken);

        var missingIds = ids
            .Except(existingIds)
            .ToList();

        var errors = missingIds
            .Select(missingId => GeneralErrors.NotFound("location", missingId))
            .ToList();

        return errors.Count != 0
            ? UnitResult.Failure(new Errors(errors))
            : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> SoftDeleteUnusedLocationsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE locations l
                           SET is_active  = false,
                               deleted_at = NOW(),
                               updated_at = NOW(),
                               deletion_reason = @deletionReason
                           FROM department_locations dl
                           WHERE dl.location_id = l.id
                             AND dl.department_id = @departmentId
                             AND NOT EXISTS(SELECT 1
                                            FROM department_locations dl2
                                                     JOIN departments d ON dl2.department_id = d.id
                                            WHERE dl2.location_id = dl.location_id
                                              AND d.is_active = true
                                              AND d.id <> @departmentId)
                             AND l.is_active = true
                           """;

        DbConnection? dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(
                sql,
                new
                {
                    departmentId = departmentId.Value,
                    deletionReason = DeletionReason.NO_ACTIVE_DEPARTMENTS.ToString(),
                });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to soft delete locations for department {DepartmentId}",
                departmentId.Value);

            return GeneralErrors.Database(message: "Не удалось пометить как мягкое удаление локации подразделения.");
        }
    }

    public async Task<UnitResult<Error>> RestoreLocationsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE locations l
                           SET is_active  = true,
                               deleted_at = NULL,
                               updated_at = NOW(),
                               deletion_reason = NULL
                           FROM department_locations dl
                           WHERE dl.location_id = l.id
                             AND dl.department_id = @departmentId
                             AND l.is_active = false
                             AND l.deletion_reason = @deletionReason
                           """;

        DbConnection? dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(
                sql,
                new
                {
                    departmentId = departmentId.Value,
                    deletionReason = DeletionReason.NO_ACTIVE_DEPARTMENTS.ToString(),
                });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to restore locations for department {DepartmentId}",
                departmentId.Value);

            return GeneralErrors.Database(message: "Не удалось восстановить локации подразделения.");
        }
    }

    public async Task<UnitResult<Error>> DeleteLocationsMarkDelete(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations
                .Where(l => l.IsActive == false && l.DeletedAt < DateTime.UtcNow.AddMonths(-1))
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete locations");

            return GeneralErrors.Database(ex.Message);
        }
    }
}