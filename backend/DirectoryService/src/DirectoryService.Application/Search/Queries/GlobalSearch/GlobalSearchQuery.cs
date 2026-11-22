using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Search.Queries.GlobalSearch;

public record GlobalSearchQuery(string? Q) : IQuery;