using DirectoryService.Application.Search.Queries.GlobalSearch;
using DirectoryService.Contracts.Search.GlobalSearch;
using Microsoft.AspNetCore.Mvc;
using SharedService.Core.Abstractions;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Presentation.Search.GlobalSearch;

[ApiController]
[Route("api/search")]
public class GlobalSearchController : ControllerBase
{
    // GET
    [HttpGet]
    public async Task<EndpointResult<PaginationEnvelope<GlobalSearchDto>>> GetAll(
        [FromServices] IQueryHandler<PaginationEnvelope<GlobalSearchDto>, GlobalSearchQuery> handler,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var query = new GlobalSearchQuery(q);

        return await handler.Handle(query, cancellationToken);
    }
}