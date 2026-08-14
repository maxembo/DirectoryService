using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentTreeRoots;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetDepartmentTreeRootsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task GetDepartmentTreeRoots_WhenRootHasActiveAndArchivedChildren_ShouldReturnOnlyActiveChildren()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var frontend =
            await CreateChildDepartment("frontend", "frontend", company, [locationId]);

        var query = new GetDepartmentTreeRootsQuery(new GetDepartmentTreeRootsRequest());

        // soft delete
        var deleteFrontendResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(frontend.Id.Value));

        Assert.True(deleteFrontendResult.IsSuccess);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.Items);

        Assert.Equal(company.Id.Value, root.Id);
        Assert.True(root.IsActive);
        Assert.True(root.HasChildren);

        Assert.Single(root.Children);
        Assert.Contains(backend.Id.Value, root.Children);
        Assert.DoesNotContain(frontend.Id.Value, root.Children);

        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetDepartmentTreeRoots_WhenRootHasActiveChild_ShouldSetHasChildrenToTrue()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend = await CreateChildDepartment("backend", "backend", company, [locationId]);

        await ClearDepartmentCache();

        var query = new GetDepartmentTreeRootsQuery(new GetDepartmentTreeRootsRequest());

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.Items);

        Assert.Equal(company.Id.Value, root.Id);
        Assert.True(root.IsActive);
        Assert.True(root.HasChildren);
        Assert.Contains(backend.Id.Value, root.Children);
        Assert.Single(root.Children);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetDepartmentTreeRoots_WhenRootHasOnlyArchivedChildren_ShouldSetHasChildrenToFalse()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var query = new GetDepartmentTreeRootsQuery(new GetDepartmentTreeRootsRequest());

        // soft delete
        var deleteBackendResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(backend.Id.Value));

        Assert.True(deleteBackendResult.IsSuccess);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.Items);

        Assert.Equal(company.Id.Value, root.Id);
        Assert.True(root.IsActive);
        Assert.False(root.HasChildren);
        Assert.DoesNotContain(backend.Id.Value, root.Children);
        Assert.Empty(root.Children);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetDepartmentTreeRoots_WhenRootDepartmentIsArchived_ShouldNotReturnArchivedDepartment()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var query = new GetDepartmentTreeRootsQuery(new GetDepartmentTreeRootsRequest());

        // soft delete
        var deleteCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(deleteCompanyResult.IsSuccess);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    private Task<Result<PaginationEnvelope<GetDepartmentTreeRootsDto>, Errors>> Execute(
        GetDepartmentTreeRootsQuery query)
        => Execute<Result<PaginationEnvelope<GetDepartmentTreeRootsDto>, Errors>,
            GetDepartmentTreeRootsHandler>(handler =>
            handler.Handle(query, CancellationToken.None));

    private Task<Result<Guid, Errors>> ExecuteSoftDelete(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}