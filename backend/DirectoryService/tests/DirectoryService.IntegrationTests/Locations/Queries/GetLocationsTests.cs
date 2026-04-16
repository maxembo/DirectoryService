using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.IntegrationTests.Locations.Queries;

public class GetLocationsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task GetLocations_WhenFilteredByDepartmentIds_ShouldReturnOnlyDepartmentLocations()
    {
        // arrange
        var locationId = await CreateLocation("location name");
        var nonDepartmentLocationId
            = await CreateLocation("location name 1", "city", "country", "street", "1");

        var department =
            await CreateParentDepartment("department name", "department", [locationId]);

        var query =
            CreateQuery([department.Id.Value], null, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(locationId.Value, result.Value.Items[0].Id);

        Assert.DoesNotContain(result.Value.Items, x => x.Id == nonDepartmentLocationId.Value);
    }

    [Fact]
    public async Task GetLocations_WhenDepartmentIdsDoNotMatchAnyLocation_ShouldReturnEmptyList()
    {
        // arrange
        await CreateLocation("location name");

        var departmentId = DepartmentId.Create(Guid.NewGuid());

        var query =
            CreateQuery([departmentId.Value], null, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_WhenDepartmentIdsAndIsActiveTrue_ShouldReturnOnlyActiveLocations()
    {
        // arrange
        var activeLocationId = await CreateLocation("location name");
        var activeLocationId1 =
            await CreateLocation("location name 1", "city", "country", "street", "1");
        var inactiveLocationId =
            await CreateLocation("location name 2", "city", "country", "street", "2");

        await MarkLocationAsDeleted(inactiveLocationId);

        var department =
            await CreateParentDepartment(
                "department name", "department", [activeLocationId, activeLocationId1, inactiveLocationId]);

        var query =
            CreateQuery([department.Id.Value], null, true, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Length);

        Assert.All(result.Value.Items, x => Assert.True(x.IsActive));

        var ids = result.Value.Items.Select(x => x.Id).ToList();

        Assert.Contains(activeLocationId.Value, ids);
        Assert.Contains(activeLocationId1.Value, ids);
        Assert.DoesNotContain(inactiveLocationId.Value, ids);
    }

    [Fact]
    public async Task GetLocations_WhenDepartmentIdsAndIsActiveFalse_ShouldReturnOnlyActiveLocations()
    {
        // arrange
        var activeLocationId = await CreateLocation("location name");
        var activeLocationId1 =
            await CreateLocation("location name 1", "city", "country", "street", "1");
        var inactiveLocationId =
            await CreateLocation("location name 2", "city", "country", "street", "2");

        await MarkLocationAsDeleted(inactiveLocationId);

        var department =
            await CreateParentDepartment(
                "department name", "department", [activeLocationId, activeLocationId1, inactiveLocationId]);

        var query =
            CreateQuery([department.Id.Value], null, false, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);

        Assert.All(result.Value.Items, x => Assert.False(x.IsActive));

        var ids = result.Value.Items.Select(x => x.Id).ToList();

        Assert.Contains(inactiveLocationId.Value, ids);
        Assert.DoesNotContain(activeLocationId.Value, ids);
        Assert.DoesNotContain(activeLocationId1.Value, ids);
    }

    [Fact]
    public async Task GetLocations_WhenSearchMatchesLocation_ShouldReturnFilteredLocations()
    {
        // arrange
        const string matchedName = "location 1";

        await CreateLocation(name: matchedName);
        await CreateLocation(
            name: "another location", city: "city", country: "country", street: "street", house: "1 house");

        var query = CreateQuery([], matchedName, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(matchedName, result.Value.Items[0].Name);
    }

    [Fact]
    public async Task GetLocations_WhenSearchDoesNotMatchAnyLocation_ShouldReturnEmptyList()
    {
        // arrange
        const string nonExistingName = "location not exists 34233";

        var query = CreateQuery([], nonExistingName, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_WhenSearchMatchesSeveralLocations_ShouldReturnFilteredLocations()
    {
        // arrange
        const string name = "location 1";
        const string name1 = "Location 2";
        const string name2 = "LOCATION 3";

        const string matchedName = "location";

        await CreateLocation(name: name);
        await CreateLocation(name: name1, city: "city", country: "country", street: "street", house: "1 house");
        await CreateLocation(name: name2, city: "city", country: "country", street: "street", house: "2 house");

        var query = CreateQuery([], matchedName, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Length);

        Assert.All(
            result.Value.Items,
            x => Assert.Contains(matchedName, x.Name, StringComparison.OrdinalIgnoreCase));

        var names = result.Value.Items.Select(x => x.Name).ToList();

        Assert.Contains(name, names);
        Assert.Contains(name1, names);
        Assert.Contains(name2, names);
        Assert.DoesNotContain("another", names);
    }

    [Fact]
    public async Task GetLocations_WhenSearchIsEmpty_ShouldReturnAllLocations()
    {
        // arrange
        const string name = "location 1";
        const string name1 = "Location 2";
        const string name2 = "LOCATION 3";

        await CreateLocation(name: name);
        await CreateLocation(name: name1, city: "city", country: "country", street: "street", house: "1 house");
        await CreateLocation(name: name2, city: "city", country: "country", street: "street", house: "2 house");

        var query = CreateQuery([], string.Empty, null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Length);

        var names = result.Value.Items.Select(x => x.Name).ToList();

        Assert.Contains(name, names);
        Assert.Contains(name1, names);
        Assert.Contains(name2, names);
    }

    [Fact]
    public async Task GetLocations_WhenSearchHasSurroundingSpaces_ShouldReturnMatchedLocations()
    {
        // arrange
        const string name = "location 1";
        const string name1 = "Location 2";
        const string name2 = "LOCATION 3";

        await CreateLocation(name: name);
        await CreateLocation(name: name1, city: "city", country: "country", street: "street", house: "1 house");
        await CreateLocation(name: name2, city: "city", country: "country", street: "street", house: "2 house");

        var query = CreateQuery([], "  location  ", null, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);

        Assert.All(
            result.Value.Items,
            x => Assert.Contains("location", x.Name, StringComparison.OrdinalIgnoreCase));

        var names = result.Value.Items.Select(x => x.Name).ToList();

        Assert.Contains(name, names);
        Assert.Contains(name1, names);
        Assert.Contains(name2, names);
    }

    [Fact]
    public async Task GetLocations_WhenIsActiveIsTrue_ShouldReturnOnlyActiveLocations()
    {
        // arrange
        var activeLocationId = await CreateLocation(name: "active location");
        var inactiveLocationId = await CreateLocation(
            name: "inactive location", city: "city", country: "country", street: "street", house: "1");

        await MarkLocationAsDeleted(inactiveLocationId);

        var query = CreateQuery([], null, true, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Items, x => Assert.True(x.IsActive));
        Assert.Contains(result.Value.Items, x => x.Id == activeLocationId.Value);
        Assert.DoesNotContain(result.Value.Items, x => x.Id == inactiveLocationId.Value);
    }

    [Fact]
    public async Task GetLocations_WhenIsActiveIsFalse_ShouldReturnOnlyInactiveLocations()
    {
        // arrange
        var activeLocationId = await CreateLocation(name: "active location");
        var inactiveLocationId = await CreateLocation(
            name: "inactive location", city: "city", country: "country", street: "street", house: "1");

        await MarkLocationAsDeleted(inactiveLocationId);

        var query = CreateQuery([], null, false, null, null);

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Items, x => Assert.False(x.IsActive));
        Assert.Contains(result.Value.Items, x => x.Id == inactiveLocationId.Value);
        Assert.DoesNotContain(result.Value.Items, x => x.Id == activeLocationId.Value);
    }

    [Fact]
    public async Task GetLocations_WhenSortByNameAsc_ShouldReturnSortedLocations()
    {
        // arrange
        const string name = "Test name";
        const string name1 = "Test name 1";
        const string name2 = "Test name 2";

        await CreateLocation(name: name);
        await CreateLocation(name: name1, city: "city", country: "country", street: "street", house: "1");
        await CreateLocation(name: name2, city: "city", country: "country", street: "street", house: "2");

        var query = CreateQuery([], null, null, "name", "asc");

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        string[] names = result.Value.Items.Select(x => x.Name).ToArray();

        Assert.Equal([name, name1, name2], names);
    }

    [Fact]
    public async Task GetLocations_WhenSortByNameDesc_ShouldReturnSortedLocations()
    {
        // arrange
        const string name = "Test name";
        const string name1 = "Test name 1";
        const string name2 = "Test name 2";

        await CreateLocation(name: name);
        await CreateLocation(name: name1, city: "city", country: "country", street: "street", house: "1");
        await CreateLocation(name: name2, city: "city", country: "country", street: "street", house: "2");

        var query = CreateQuery([], null, null, "name", "desc");

        // act
        var result = await Execute(query);

        // assert
        Assert.True(result.IsSuccess);

        string[] names = result.Value.Items.Select(x => x.Name).ToArray();

        Assert.Equal([name2, name1, name], names);
    }

    private static GetLocationsQuery CreateQuery(
        Guid[]? departmentIds,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection)
    {
        return new GetLocationsQuery(new GetLocationsRequest(departmentIds, search, isActive, sortBy, sortDirection));
    }

    private Task<Result<PaginationEnvelope<GetLocationsDto>, Errors>> Execute(GetLocationsQuery query)
        => Execute<Result<PaginationEnvelope<GetLocationsDto>, Errors>, GetLocationsHandler>(
            handler => handler.Handle(query, CancellationToken.None));
}