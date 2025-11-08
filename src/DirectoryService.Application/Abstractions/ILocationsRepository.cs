using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Abstractions;

public interface ILocationsRepository
{
    Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken = default);
}