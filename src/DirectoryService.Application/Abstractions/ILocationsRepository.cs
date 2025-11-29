using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);

    Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    
    Task<UnitResult<Errors>> CheckExistingAndActiveIdsAsync(Guid[] ids, CancellationToken cancellationToken = default);
}