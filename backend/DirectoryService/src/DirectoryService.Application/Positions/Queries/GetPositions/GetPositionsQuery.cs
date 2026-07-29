using DirectoryService.Contracts.Positions.GetPositions;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public record GetPositionsQuery(GetPositionsRequest Request) : IQuery;