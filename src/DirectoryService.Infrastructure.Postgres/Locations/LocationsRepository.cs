using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

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

        var saveChangesResult = await _dbContext.SaveChangesResultAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        return location.Id.Value;
    }

    public async Task<Result<Location, Error>> GetByIdAsync(
        LocationId locationId, CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location == null)
            return GeneralErrors.NotFound(locationId.Value, "location");

        return location;
    }

    public async Task<UnitResult<Errors>> CheckExistingAndActiveIdsAsync(
        Guid[] ids, CancellationToken cancellationToken = default)
    {
        var locationIds = LocationId.Create(ids);

        var existingIds = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id) && l.IsActive == true)
            .Select(l => l.Id.Value)
            .ToListAsync(cancellationToken);

        var missingIds = ids
            .Except(existingIds)
            .ToList();

        var errors = missingIds
            .Select(missingId => GeneralErrors.NotFound(missingId, "location"))
            .ToList();

        return errors.Count != 0
            ? UnitResult.Failure(new Errors(errors))
            : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> DeleteUnusedLocationsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE locations l
                           SET is_active  = false,
                               deleted_at = NOW()
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

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(sql, param: new { departmentId = departmentId.Value });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete locations");

            return GeneralErrors.Database(null, ex.Message);
        }
    }
}