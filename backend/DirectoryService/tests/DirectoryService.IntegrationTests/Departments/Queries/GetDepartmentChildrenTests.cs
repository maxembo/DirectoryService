using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentChildren;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetDepartmentChildrenTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task GetDepartmentChildren_WhenDirectChildIsArchived_ShouldNotReturnArchivedChild()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var query = new GetDepartmentChildrenQuery(company.Id.Value, new GetDepartmentChildrenRequest());

        // soft delete
        var deleteTeamResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(backend.Id.Value));

        Assert.True(deleteTeamResult.IsSuccess);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenChildHasActiveDescendant_ShouldSetHasChildrenToTrue()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        await CreateChildDepartment("team", "team", backend, [locationId]);

        var query = new GetDepartmentChildrenQuery(company.Id.Value, new GetDepartmentChildrenRequest());

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var child = Assert.Single(result.Value.Items);

        Assert.Equal(backend.Id.Value, child.Id);
        Assert.Equal(company.Id.Value, child.ParentId);
        Assert.True(child.IsActive);
        Assert.True(child.HasChildren);

        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenChildHasOnlyArchivedDescendants_ShouldSetHasChildrenToFalse()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var team =
            await CreateChildDepartment("team", "team", backend, [locationId]);

        var query = new GetDepartmentChildrenQuery(company.Id.Value, new GetDepartmentChildrenRequest());

        // soft delete
        var deleteTeamResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(team.Id.Value));

        Assert.True(deleteTeamResult.IsSuccess);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var child = Assert.Single(result.Value.Items);

        Assert.Equal(backend.Id.Value, child.Id);
        Assert.Equal(company.Id.Value, child.ParentId);
        Assert.True(child.IsActive);
        Assert.False(child.HasChildren);

        Assert.Equal(1, result.Value.TotalCount);
    }

    private Task<Result<PaginationEnvelope<GetDepartmentChildrenDto>, Errors>> Execute(GetDepartmentChildrenQuery query)
        => Execute<Result<PaginationEnvelope<GetDepartmentChildrenDto>, Errors>,
            GetDepartmentChildrenHandler>(handler =>
            handler.Handle(query, CancellationToken.None));

    private Task<Result<Guid, Errors>> ExecuteSoftDelete(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}