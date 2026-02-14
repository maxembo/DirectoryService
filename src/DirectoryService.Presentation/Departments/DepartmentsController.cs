using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.MoveDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Application.Departments.Commands.UpdateDepartments;
using DirectoryService.Application.Departments.Queries.GetChildrenDepartments;
using DirectoryService.Application.Departments.Queries.GetRootDepartments;
using DirectoryService.Application.Departments.Queries.GetTopFiveDepartmentsWithMostPositions;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions;
using Shared.EndpointResults;
using Shared.Response;
using GetDepartmentDto = DirectoryService.Contracts.Departments.GetDepartments.Dtos.GetDepartmentDto;

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

    [HttpGet("top-positions")]
    public async Task<EndpointResult<Contracts.Departments.GetTopFiveDepartmentsWithMostPositions.Dtos.GetDepartmentDto
            []>>
        GetTopFiveDepartmentsWithMostPositions(
            [FromServices] GetTopFiveDepartmentsWithMostPositionsHandler handler,
            CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }

    [HttpGet("top-positions/dapper")]
    public async Task<EndpointResult<Contracts.Departments.GetTopFiveDepartmentsWithMostPositions.Dtos.GetDepartmentDto
            []>>
        GetTopFiveDepartmentsWithMostPositionsDapper(
            [FromServices] GetTopFiveDepartmentsWithMostPositionsHandlerDapper handler,
            CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }

    [HttpGet("roots")]
    public async Task<EndpointResult<PaginationEnvelope<GetDepartmentDto>>> GetRoots(
        [FromServices] IQueryHandler<PaginationEnvelope<GetDepartmentDto>, GetRootDepartmentsQuery> handler,
        [FromQuery] GetRootDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetRootDepartmentsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{parentId:guid}/children")]
    public async Task<EndpointResult<PaginationEnvelope<GetDepartmentDto>>> GetChildrens(
        Guid parentId,
        [FromServices] IQueryHandler<PaginationEnvelope<GetDepartmentDto>, GetChildrenDepartmentsQuery> handler,
        [FromQuery] GetChildrenDepartmentsRequest departmentsRequest,
        CancellationToken cancellationToken)
    {
        var query = new GetChildrenDepartmentsQuery(parentId, departmentsRequest);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);

        return await handler.Handle(command, cancellationToken);
    }
}