using DirectoryService.Application.Positions.Commands.CreatePositions;
using DirectoryService.Application.Positions.Commands.SoftDeletePositions;
using DirectoryService.Application.Positions.Commands.UpdatePositions;
using DirectoryService.Application.Positions.Queries.GetPositions;
using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Contracts.Positions.GetPositions;
using DirectoryService.Contracts.Positions.UpdatePositions;
using Microsoft.AspNetCore.Mvc;
using SharedService.Core.Abstractions;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("/api/positions")]
public class PositionsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginationEnvelope<GetPositionsDto>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(409)]
    public async Task<EndpointResult<PaginationEnvelope<GetPositionsDto>>> Get(
        [FromServices] IQueryHandler<PaginationEnvelope<GetPositionsDto>, GetPositionsQuery> handler,
        [FromQuery] GetPositionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetPositionsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(201)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(409)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{positionId:guid}")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(409)]
    public async Task<EndpointResult<Guid>> Update(
        [FromRoute] Guid positionId,
        [FromServices] ICommandHandler<Guid, UpdatePositionCommand> handler,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePositionCommand(positionId, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{positionId:guid}")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(400)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(409)]
    public async Task<EndpointResult<Guid>> MarkAsDeleted(
        [FromRoute] Guid positionId,
        [FromServices] ICommandHandler<Guid, SoftDeletePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeletePositionCommand(positionId);

        return await handler.Handle(command, cancellationToken);
    }
}