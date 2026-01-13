using DirectoryService.Application.Departments.MoveDepartments;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DirectoryBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }

    [Fact]
    public async Task MoveDepartmentToSelfShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(company.Id.Value, new MoveDepartmentRequest(company.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentToChildShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(company.Id.Value, new MoveDepartmentRequest(dev.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentNotFoundShouldFailed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var nonExistingId = Guid.NewGuid();

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(nonExistingId, new MoveDepartmentRequest(null));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentParentNotFoundShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var nonExistingParentId = Guid.NewGuid();

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(dev.Id.Value, new MoveDepartmentRequest(nonExistingParentId));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentWithoutParentShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var fronted = await CreateChildDepartment("фротендеры", "frontend", dev, [locationId]);

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(dev.Id.Value, new MoveDepartmentRequest(null));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await ExecuteInDb(
            async dbContext =>
            {
                var departmentDev = await dbContext.Departments.FirstAsync(d => d.Id == dev.Id, cancellationToken);

                Assert.NotNull(departmentDev);
                Assert.Equal("dev", departmentDev.Path.Value);
                Assert.Equal(0, departmentDev.Path.Depth);
                Assert.Null(departmentDev.ParentId);

                var departmentFronted = await dbContext.Departments.FirstAsync(
                    d => d.Id == fronted.Id, cancellationToken);

                Assert.NotNull(departmentFronted);
                Assert.Equal("dev.frontend", departmentFronted.Path.Value);
                Assert.Equal(1, departmentFronted.Path.Depth);
                Assert.Equal(dev.Id.Value, departmentFronted.ParentId.Value);
            });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task MoveDepartmentWithValidDataShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var fronted = await CreateChildDepartment("фронтендеры", "fronted", dev, [locationId]);

        var backend = await CreateChildDepartment("бекендеры", "backend", dev, [locationId]);

        var team = await CreateChildDepartment("команда 1", "team", fronted, [locationId]);

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteMoveDepartmentHandler(
            sut =>
            {
                var command = new MoveDepartmentCommand(fronted.Id.Value, new MoveDepartmentRequest(company.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await ExecuteInDb(
            async dbContext =>
            {
                var departmentFronted = await dbContext.Departments.FirstAsync(
                    d => d.Id == fronted.Id, cancellationToken);

                Assert.NotNull(departmentFronted);
                Assert.Equal("company.fronted", departmentFronted.Path.Value);
                Assert.Equal(1, departmentFronted.Path.Depth);
                Assert.Equal(company.Id.Value, departmentFronted.ParentId!.Value);

                var departmentTeam = await dbContext.Departments.FirstAsync(d => d.Id == team.Id, cancellationToken);

                Assert.NotNull(departmentTeam);
                Assert.Equal("company.fronted.team", departmentTeam.Path.Value);
                Assert.Equal(2, departmentTeam.Path.Depth);
                Assert.Equal(departmentFronted.Id.Value, departmentTeam.ParentId!.Value);
            });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    private async Task<T> ExecuteMoveDepartmentHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();

        return await action(sut);
    }
}