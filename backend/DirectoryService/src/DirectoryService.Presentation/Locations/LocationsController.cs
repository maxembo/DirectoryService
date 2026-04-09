using DirectoryService.Application.Locations.Commands.CreateLocations;
using DirectoryService.Application.Locations.Commands.SoftDeleteLocations;
using DirectoryService.Application.Locations.Commands.UpdateLocations;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using DirectoryService.Contracts.Locations.UpdateLocations;
using Microsoft.AspNetCore.Mvc;
using SharedService.Core.Abstractions;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel.Response;

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
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    [ProducesResponseType<Envelope<GetLocationsDto>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<PaginationEnvelope<GetLocationsDto>>> GetLocations(
        [FromServices] IQueryHandler<PaginationEnvelope<GetLocationsDto>, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("dapper")]
    [ProducesResponseType<Envelope<GetLocationsDto>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<PaginationEnvelope<GetLocationsDto>>> GetLocationsDapper(
        [FromServices] GetLocationsHandlerDapper handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpPatch("{locationId:guid}")]
    public async Task<EndpointResult<Guid>> Patch(
        Guid locationId,
        [FromServices] ICommandHandler<Guid, UpdateLocationCommand> handler,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(locationId, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        Guid locationId,
        [FromServices] ICommandHandler<Guid, SoftDeleteLocationCommand> handler,
        CancellationToken cancellationToke)
    {
        var command = new SoftDeleteLocationCommand(locationId);

        return await handler.Handle(command, cancellationToke);
    }
}