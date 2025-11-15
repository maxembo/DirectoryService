using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationRequest> handler,
        [FromBody] CreateLocationDto locationDto,
        CancellationToken cancellationToken)
    {
        var request = new CreateLocationRequest(locationDto);

        return await handler.Handle(request, cancellationToken);
    }
}