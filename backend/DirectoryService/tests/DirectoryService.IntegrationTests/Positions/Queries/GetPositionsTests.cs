using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Queries.GetPositions;
using DirectoryService.Contracts.Positions.GetPositions;
using DirectoryService.IntegrationTests.Infrastructure;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.IntegrationTests.Positions.Queries;

public class GetPositionsTests(DirectoryTestWebFactory factory) : PositionBaseTests(factory)
{
    [Fact]
    public async Task GetPositions_WithoutFilters_ShouldReturnAllPositions()
    {
        // arrange
        var department = await SeedActiveDepartment();

        var activePositionId = await CreatePosition(
            "Active position",
            "Active position description",
            [department.Id]);

        var inactivePositionId = await CreatePosition(
            "Inactive position",
            "Inactive position description",
            [department.Id]);

        await MarkPositionAsDeleted(inactivePositionId);

        var query = CreateQuery(sortBy: "name", sortDirection: "asc");

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Length);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(20, result.Value.PageSize);

        var activePosition = Assert.Single(
            result.Value.Items,
            position => position.Id == activePositionId.Value);

        Assert.Equal("Active position", activePosition.Name);
        Assert.Equal("Active position description", activePosition.Description);
        Assert.True(activePosition.IsActive);
        Assert.Null(activePosition.DeletedAt);
        Assert.NotEqual(default, activePosition.CreatedAt);
        Assert.NotEqual(default, activePosition.UpdatedAt);

        var inactivePosition = Assert.Single(
            result.Value.Items,
            position => position.Id == inactivePositionId.Value);

        Assert.False(inactivePosition.IsActive);
        Assert.NotNull(inactivePosition.DeletedAt);
    }

    [Fact]
    public async Task GetPositions_WhenSearchMatchesName_ShouldReturnMatchingPositions()
    {
        // arrange
        var department = await SeedActiveDepartment();

        var seniorDeveloperId = await CreatePosition("Senior Developer", "description", [department.Id]);

        var juniorDeveloperId = await CreatePosition("Junior developer", "description", [department.Id]);

        await CreatePosition("Accountant", "description", [department.Id]);

        var query = CreateQuery(search: "DEVELOPER", sortBy: "name", sortDirection: "asc");

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Length);

        var positionIds = result.Value.Items.Select(position => position.Id).ToArray();

        Assert.Contains(seniorDeveloperId.Value, positionIds);
        Assert.Contains(juniorDeveloperId.Value, positionIds);
        Assert.All(
            result.Value.Items,
            position => Assert.Contains("developer", position.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetPositions_WhenSearchDoesNotMatch_ShouldReturnEmptyResult()
    {
        // arrange
        var department = await SeedActiveDepartment();

        await CreatePosition("Developer", "description", [department.Id]);

        var query = CreateQuery(search: "non-existing-position");

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetPositions_WhenFilteredByDepartmentIds_ShouldReturnRelatedPositions()
    {
        // arrange
        var firstDepartment = await SeedActiveDepartment("first", "first-department");
        var secondDepartment = await SeedActiveDepartment("second", "second-department");

        var firstPositionId =
            await CreatePosition("First department position", "description", [firstDepartment.Id]);

        var sharedPositionId = await CreatePosition(
            "Shared position", "description", [firstDepartment.Id, secondDepartment.Id]);

        var secondPositionId =
            await CreatePosition("Second department position", "description", [secondDepartment.Id]);

        var query = CreateQuery([firstDepartment.Id.Value]);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Length);

        var positionIds = result.Value.Items.Select(position => position.Id).ToArray();

        Assert.Contains(firstPositionId.Value, positionIds);
        Assert.Contains(sharedPositionId.Value, positionIds);
        Assert.DoesNotContain(secondPositionId.Value, positionIds);
    }

    [Fact]
    public async Task GetPositions_WhenPositionMatchesSeveralDepartmentIds_ShouldReturnItOnlyOnce()
    {
        // arrange
        var firstDepartment = await SeedActiveDepartment("first", "first-department");
        var secondDepartment = await SeedActiveDepartment("second", "second-department");

        var positionId = await CreatePosition(
            "Shared position",
            "description",
            [firstDepartment.Id, secondDepartment.Id]);

        var query = CreateQuery([firstDepartment.Id.Value, secondDepartment.Id.Value]);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);

        var position = Assert.Single(result.Value.Items);
        Assert.Equal(positionId.Value, position.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPositions_WhenFilteredByActivity_ShouldReturnPositionsWithRequestedStatus(
        bool requestedActivity)
    {
        // arrange
        var department = await SeedActiveDepartment();

        var activePositionId = await CreatePosition("Active position", "description", [department.Id]);

        var inactivePositionId = await CreatePosition("Inactive position", "description", [department.Id]);

        await MarkPositionAsDeleted(inactivePositionId);

        var query = CreateQuery(isActive: requestedActivity);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);

        var position = Assert.Single(result.Value.Items);

        Assert.Equal(requestedActivity, position.IsActive);
        Assert.Equal(
            requestedActivity ? activePositionId.Value : inactivePositionId.Value,
            position.Id);
    }

    [Fact]
    public async Task GetPositions_WithCombinedFilters_ShouldReturnMatchingPositions()
    {
        // arrange
        var firstDepartment = await SeedActiveDepartment("first", "first-department");
        var secondDepartment = await SeedActiveDepartment("second", "second-department");

        var expectedPositionId = await CreatePosition("Senior Developer", "description", [firstDepartment.Id]);

        var inactivePositionId =
            await CreatePosition("Inactive Developer", "description", [firstDepartment.Id]);

        await MarkPositionAsDeleted(inactivePositionId);

        await CreatePosition("Accountant", "description", [firstDepartment.Id]);
        await CreatePosition("Developer from another department", "description", [secondDepartment.Id]);

        var query = CreateQuery(
            [firstDepartment.Id.Value],
            "developer",
            isActive: true);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);

        var position = Assert.Single(result.Value.Items);
        Assert.Equal(expectedPositionId.Value, position.Id);
        Assert.True(position.IsActive);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task GetPositions_WhenSortedByName_ShouldReturnExpectedOrder(string sortDirection)
    {
        // arrange
        var department = await SeedActiveDepartment();

        await CreatePosition("Bravo position", "description", [department.Id]);
        await CreatePosition("Alpha position", "description", [department.Id]);
        await CreatePosition("Charlie position", "description", [department.Id]);

        var query = CreateQuery(sortBy: "name", sortDirection: sortDirection);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        string[] actualNames = result.Value.Items.Select(position => position.Name).ToArray();

        string[] expectedNames = sortDirection == "asc"
            ? ["Alpha position", "Bravo position", "Charlie position"]
            : ["Charlie position", "Bravo position", "Alpha position"];

        Assert.Equal(expectedNames, actualNames);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task GetPositions_WhenSortedByDepartmentCount_ShouldReturnExpectedOrder(string sortDirection)
    {
        // arrange
        var firstDepartment = await SeedActiveDepartment("first", "first-department");
        var secondDepartment = await SeedActiveDepartment("second", "second-department");
        var thirdDepartment = await SeedActiveDepartment("third", "third-department");

        await CreatePosition("One department", "description", [firstDepartment.Id]);
        await CreatePosition("Two departments", "description", [firstDepartment.Id, secondDepartment.Id]);
        await CreatePosition(
            "Three departments",
            "description",
            [firstDepartment.Id, secondDepartment.Id, thirdDepartment.Id]);

        var query = CreateQuery(sortBy: "department_count", sortDirection: sortDirection);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        string[] actualNames = result.Value.Items.Select(position => position.Name).ToArray();

        string[] expectedNames = sortDirection == "asc"
            ? ["One department", "Two departments", "Three departments"]
            : ["Three departments", "Two departments", "One department"];

        Assert.Equal(expectedNames, actualNames);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task GetPositions_WhenSortedByStatus_ShouldReturnExpectedOrder(string sortDirection)
    {
        // arrange
        var department = await SeedActiveDepartment();

        var activePositionId = await CreatePosition("Active position", "description", [department.Id]);

        var inactivePositionId = await CreatePosition("Inactive position", "description", [department.Id]);

        await MarkPositionAsDeleted(inactivePositionId);

        var query = CreateQuery(sortBy: "status", sortDirection: sortDirection);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        var actualIds = result.Value.Items.Select(position => position.Id).ToArray();

        Guid[] expectedIds = sortDirection == "asc"
            ? [inactivePositionId.Value, activePositionId.Value]
            : [activePositionId.Value, inactivePositionId.Value];

        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public async Task GetPositions_WhenPageRequested_ShouldReturnRequestedPageAndTotalCount()
    {
        // arrange
        var department = await SeedActiveDepartment();

        await CreatePosition("Alpha position", "description", [department.Id]);
        await CreatePosition("Bravo position", "description", [department.Id]);
        await CreatePosition("Charlie position", "description", [department.Id]);
        await CreatePosition("Delta position", "description", [department.Id]);
        await CreatePosition("Echo position", "description", [department.Id]);

        var query = CreateQuery(
            sortBy: "name",
            sortDirection: "asc",
            page: 2,
            pageSize: 2);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Length);

        string[] names = result.Value.Items.Select(position => position.Name).ToArray();
        Assert.Equal(["Charlie position", "Delta position"], names);
    }

    [Fact]
    public async Task GetPositions_WhenRequestedPageIsEmpty_ShouldReturnCorrectTotalCount()
    {
        // arrange
        var department = await SeedActiveDepartment();

        await CreatePosition("Alpha position", "description", [department.Id]);
        await CreatePosition("Bravo position", "description", [department.Id]);
        await CreatePosition("Charlie position", "description", [department.Id]);
        await CreatePosition("Delta position", "description", [department.Id]);
        await CreatePosition("Echo position", "description", [department.Id]);

        var query = CreateQuery(
            sortBy: "name",
            sortDirection: "asc",
            page: 4,
            pageSize: 2);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(4, result.Value.Page);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages);
    }

    [Theory]
    [InlineData(0, 20, "page")]
    [InlineData(1, 0, "pageSize")]
    [InlineData(1, 101, "pageSize")]
    public async Task GetPositions_WhenPaginationIsInvalid_ShouldFail(
        int page,
        int pageSize,
        string invalidField)
    {
        // arrange
        var query = CreateQuery(page: page, pageSize: pageSize);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Error);
        var expectedError = GeneralErrors.Invalid(invalidField);

        Assert.Equal(expectedError.Code, error.Code);
        Assert.Equal(expectedError.Type, error.Type);
        Assert.Equal(expectedError.InvalidField, error.InvalidField);
    }

    [Fact]
    public async Task GetPositions_WhenSearchExceedsMaxLength_ShouldFail()
    {
        // arrange
        string search = new('s', 1001);
        var query = CreateQuery(search: search);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Error);
        var expectedError = GeneralErrors.Invalid("search");

        Assert.Equal(expectedError.Code, error.Code);
        Assert.Equal(expectedError.Type, error.Type);
        Assert.Equal(expectedError.InvalidField, error.InvalidField);
    }

    private static GetPositionsQuery CreateQuery(
        Guid[]? departmentIds = null,
        string? search = null,
        string? sortBy = null,
        string? sortDirection = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        var request = new GetPositionsRequest(
            departmentIds,
            search,
            sortBy,
            sortDirection,
            isActive) { Page = page, PageSize = pageSize };

        return new GetPositionsQuery(request);
    }

    private Task<Result<PaginationEnvelope<GetPositionsDto>, Errors>> Execute(GetPositionsQuery query) =>
        Execute<Result<PaginationEnvelope<GetPositionsDto>, Errors>, GetPositionsHandler>(handler =>
            handler.Handle(query, CancellationToken.None));
}