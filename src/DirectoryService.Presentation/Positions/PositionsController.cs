using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePositions;
using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("/api/positions")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        return await handler.Handle(command, cancellationToken);
    }
}