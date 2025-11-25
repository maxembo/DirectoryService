using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Locations;
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
        string name = location.Name.Value;
        string? address = location.Address.ToString();
        try
        {
            await _dbContext.AddAsync(location, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Location {Location.Id} has been added.", location.Id);

            return location.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("name"))
                {
                    return GeneralErrors.AlreadyExist(name);
                }

                if (pgEx.ConstraintName.Contains("address"))
                {
                    return GeneralErrors.AlreadyExist(address);
                }
            }

            _logger.LogError(ex, "Error adding a location with a name {name}", name);
            return GeneralErrors.Database();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create location!");
            return GeneralErrors.Database();
        }
    }
}