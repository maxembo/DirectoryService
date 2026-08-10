using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.MoveDepartments;
using DirectoryService.Application.Departments.Commands.RestoreDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Application.Departments.Commands.UpdateDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentChildren;
using DirectoryService.Application.Departments.Queries.GetDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentTreeRoots;
using DirectoryService.Application.Departments.Queries.GetTopFiveDepartmentsWithMostPositions;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedService.Core.Abstractions;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel.Response;
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
    public async Task<EndpointResult<Guid>> CreateDepartment(
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

    [HttpGet("tree")]
    [EndpointSummary("Получить корневые подразделения дерева")]
    [EndpointDescription(
        "Возвращает только корневой уровень дерева подразделений. " +
        "Дочерние подразделения загружаются отдельным запросом.")]
    public async Task<EndpointResult<PaginationEnvelope<GetDepartmentTreeRootsDto>>> GetTreeRoots(
        [FromServices] IQueryHandler<PaginationEnvelope<GetDepartmentTreeRootsDto>, GetDepartmentTreeRootsQuery> handler,
        [FromQuery] GetDepartmentTreeRootsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentTreeRootsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{parentId:guid}/children")]
    [EndpointSummary("Получить прямые дочерние подразделения")]
    [EndpointDescription(
        "Возвращает только непосредственных детей выбранного подразделения. " +
        "Вложенные уровни загружаются отдельными запросами при раскрытии дерева.")]
    public async Task<EndpointResult<PaginationEnvelope<GetDepartmentChildrenDto>>> GetChildren(
        Guid parentId,
        [FromServices] IQueryHandler<PaginationEnvelope<GetDepartmentChildrenDto>, GetDepartmentChildrenQuery> handler,
        [FromQuery] GetDepartmentChildrenRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentChildrenQuery(parentId, request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationEnvelope<DepartmentShortDto>>> GetDepartments(
        [FromServices] IQueryHandler<PaginationEnvelope<DepartmentShortDto>, GetDepartmentsQuery> handler,
        [FromQuery] GetDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> DeleteDepartment(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);

        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPatch("{departmentId:guid}/restore")]
    public async Task<EndpointResult<Guid>> Restore(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, RestoreDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new RestoreDepartmentCommand(departmentId);

        return await handler.Handle(command, cancellationToken);
    }
}