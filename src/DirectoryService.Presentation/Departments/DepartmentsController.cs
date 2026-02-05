using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.MoveDepartments;
using DirectoryService.Application.Departments.Commands.UpdateDepartments;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Abstractions;
using Shared.EndpointResults;
using Shared.Response;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("/api/departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var departmentCommand = new CreateDepartmentCommand(request);

        return await handler.Handle(departmentCommand, cancellationToken);
    }

    [HttpPatch("{departmentId:guid}/locations")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocationIds(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationIdsCommand> handler,
        [FromBody] UpdateDepartmentLocationIdsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationIdsCommand(departmentId, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId:guid}/parent")]
    public async Task<EndpointResult<Guid>> MoveDepartment(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, MoveDepartmentCommand> handler,
        [FromBody] MoveDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MoveDepartmentCommand(departmentId, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("/top-positions")]
    public async Task<EndpointResult<GetDepartmentDto[]>> GetTopFiveDepartmentsWithMostPositions(
        [FromServices] IQueryHandler<GetDepartmentDto[]> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
}