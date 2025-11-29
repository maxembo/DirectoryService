using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);
}