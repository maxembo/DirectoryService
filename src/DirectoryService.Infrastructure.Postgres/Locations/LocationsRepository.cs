using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
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
}