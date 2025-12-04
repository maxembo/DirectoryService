using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartments;
using DirectoryService.Application.Departments.UpdateDepartments;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

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
}