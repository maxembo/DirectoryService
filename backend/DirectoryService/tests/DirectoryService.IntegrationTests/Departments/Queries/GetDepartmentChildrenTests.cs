using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Queries.GetDepartmentChildren;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetDepartmentChildrenTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Theory]
    [InlineData(0, 10, "page")]
    [InlineData(2, 0, "pageSize")]
    [InlineData(1, 101, "pageSize")]
    public async Task GetDepartmentChildren_WhenPaginationIsInvalid_ShouldFailValidation(
        int page, int pageSize, string invalidField)
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        await CreateChildDepartment("backend", "backend", company, [locationId]);

        var query = CreateQuery(company.Id.Value, page, pageSize);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e.InvalidField == invalidField);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenParentIdIsEmpty_ShouldFailValidation()
    {
        // arrange
        var query = CreateQuery(Guid.Empty);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { InvalidField: "department.parentId" });
    }

    [Fact]
    public async Task
        GetDepartmentChildren_WhenSomeChildrenAreArchived_ShouldReturnOnlyActiveChildrenWithCorrectPagination()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var frontend = await CreateChildDepartment("frontend", "frontend", company, [locationId]);

        var devOps = await CreateChildDepartment("devOps", "devOps", company, [locationId]);

        var director = await CreateChildDepartment("director", "director", company, [locationId]);

        var fullstack = await CreateChildDepartment("fullstack", "fullstack", company, [locationId]);

        await MarkDepartmentAsDeleted(frontend.Id);
        await MarkDepartmentAsDeleted(fullstack.Id);

        var query = CreateQuery(company.Id.Value);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Equal(3, result.Value.TotalCount);

        Assert.DoesNotContain(result.Value.Items, child => child.Id == frontend.Id.Value);
        Assert.DoesNotContain(result.Value.Items, child => child.Id == fullstack.Id.Value);

        Assert.Contains(result.Value.Items, child => child.Id == devOps.Id.Value);
        Assert.Contains(result.Value.Items, child => child.Id == director.Id.Value);
        Assert.Contains(result.Value.Items, child => child.Id == backend.Id.Value);

        Assert.Equal(1, result.Value.TotalPages);

        Assert.Equal(3, result.Value.Items.Length);
        Assert.All(result.Value.Items, child => Assert.True(child.IsActive));
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenDifferentParentsRequested_ShouldReturnChildrenOfRequestedParent()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var office = await CreateParentDepartment("office", "office", [locationId]);

        var sales = await CreateChildDepartment("sales", "sales", office, [locationId]);

        var companyQuery = CreateQuery(company.Id.Value);
        var officeQuery = CreateQuery(office.Id.Value);

        // act
        var resultCompany = await Execute(companyQuery);
        var resultOffice = await Execute(officeQuery);

        // assert
        Assert.True(resultCompany.IsSuccess);
        Assert.True(resultOffice.IsSuccess);

        var companyChild = Assert.Single(resultCompany.Value.Items);
        var officeChild = Assert.Single(resultOffice.Value.Items);

        Assert.Equal(backend.Id.Value, companyChild.Id);
        Assert.Equal(company.Id.Value, companyChild.ParentId);

        Assert.Equal(sales.Id.Value, officeChild.Id);
        Assert.Equal(office.Id.Value, officeChild.ParentId);

        Assert.DoesNotContain(resultCompany.Value.Items, child => child.Id == office.Id.Value);
        Assert.DoesNotContain(resultOffice.Value.Items, child => child.Id == company.Id.Value);

        Assert.Contains(resultCompany.Value.Items, child => child.ParentId == company.Id.Value);
        Assert.Contains(resultOffice.Value.Items, child => child.ParentId == office.Id.Value);

        Assert.Equal(1, resultCompany.Value.TotalCount);
        Assert.Equal(1, resultOffice.Value.TotalCount);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenSeveralPagesRequested_ShouldReturnDifferentChildren()
    {
        // arrange
        const int childrenCount = 25;

        var companyId = await CreateParentDepartmentWithChildren(childrenCount);

        var queryPageOne = CreateQuery(companyId.Value, 1, 10);
        var queryPageTwo = CreateQuery(companyId.Value, 2, 10);
        var queryPageThree = CreateQuery(companyId.Value, 3, 10);

        // act
        var resultPageOne = await Execute(queryPageOne);
        var resultPageTwo = await Execute(queryPageTwo);
        var resultPageThree = await Execute(queryPageThree);

        // assert
        Assert.Equal(10, resultPageOne.Value.Items.Length);
        Assert.Equal(10, resultPageTwo.Value.Items.Length);
        Assert.Equal(5, resultPageThree.Value.Items.Length);

        var ids = resultPageOne.Value.Items
            .Concat(resultPageTwo.Value.Items)
            .Concat(resultPageThree.Value.Items)
            .Select(department => department.Id)
            .ToArray();

        Assert.Equal(25, ids.Length);
        Assert.Equal(25, ids.Distinct().Count());
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenSecondPageRequested_ShouldReturnCorrectPage()
    {
        // arrange
        const int childrenCount = 25;

        const int page = 2;
        const int pageSize = 10;

        var companyId = await CreateParentDepartmentWithChildren(childrenCount);

        var query = CreateQuery(companyId.Value, page, pageSize);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Equal(page, result.Value.Page);

        Assert.Equal(pageSize, result.Value.Items.Length);

        Assert.Equal(childrenCount, result.Value.TotalCount);

        Assert.Equal(3, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenParentHasNoChildren_ShouldReturnEmptyPage()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var query = CreateQuery(company.Id.Value);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetDepartmentChildren_ShouldReturnOnlyDirectChildren()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var backendTeam =
            await CreateChildDepartment("backendTeam", "backend-team", backend, [locationId]);

        var frontend =
            await CreateChildDepartment("frontend", "frontend", company, [locationId]);

        var frontendTeam =
            await CreateChildDepartment("frontendTeam", "frontend-team", frontend, [locationId]);

        var query = CreateQuery(company.Id.Value);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Contains(result.Value.Items, child => child.ParentId == company.Id.Value);

        Assert.Equal(2, result.Value.TotalCount);

        Assert.Contains(result.Value.Items, child => child.Id == frontend.Id.Value);
        Assert.Contains(result.Value.Items, child => child.Id == backend.Id.Value);

        Assert.DoesNotContain(result.Value.Items, child => child.Id == frontendTeam.Id.Value);
        Assert.DoesNotContain(result.Value.Items, child => child.Id == backendTeam.Id.Value);

        Assert.Contains(result.Value.Items, child => child.HasChildren);
    }

    [Fact]
    public async Task GetDepartmentChildren_WhenDirectChildIsArchived_ShouldNotReturnArchivedChild()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        await MarkDepartmentAsDeleted(backend.Id);

        var query = CreateQuery(company.Id.Value);

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

        var query = CreateQuery(company.Id.Value);

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

        await MarkDepartmentAsDeleted(team.Id);

        var query = CreateQuery(company.Id.Value);

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

    private static GetDepartmentChildrenQuery CreateQuery(
        Guid parentId,
        int page = 1,
        int pageSize = 20) =>
        new(
            parentId,
            new GetDepartmentChildrenRequest { Page = page, PageSize = pageSize, });

    private Task<Result<PaginationEnvelope<GetDepartmentChildrenDto>, Errors>> Execute(GetDepartmentChildrenQuery query)
        => Execute<Result<PaginationEnvelope<GetDepartmentChildrenDto>, Errors>,
            GetDepartmentChildrenHandler>(handler =>
            handler.Handle(query, CancellationToken.None));
}