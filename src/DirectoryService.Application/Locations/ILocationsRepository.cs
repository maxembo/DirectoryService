using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);

    Task<Result<Location, Error>> GetByIdAsync(LocationId locationId, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> CheckExistingAndActiveIdsAsync(Guid[] ids, CancellationToken cancellationToken = default);
}