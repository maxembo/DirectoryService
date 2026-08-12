using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.MoveDepartments;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class MoveDepartmentTests : DirectoryBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }

    [Fact]
    public async Task MoveDepartmentToSelfShouldFailed()
    {
        // arrange
        LocationId? locationId = await CreateLocation("Location 1");

        Department? company = await CreateParentDepartment("компания it", "company", [locationId]);

        // act
        Result<Guid, Errors> result = await Execute(
            new MoveDepartmentCommand(company.Id.Value, new MoveDepartmentRequest(company.Id.Value)));

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentToChildShouldFailed()
    {
        // arrange
        LocationId? locationId = await CreateLocation("Location 1");

        Department? company = await CreateParentDepartment("компания it", "company", [locationId]);

        Department? dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        // act
        Result<Guid, Errors> result = await Execute(
            new MoveDepartmentCommand(company.Id.Value, new MoveDepartmentRequest(dev.Id.Value)));

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentNotFoundShouldFailed()
    {
        // arrange
        var nonExistingId = Guid.NewGuid();

        // act
        Result<Guid, Errors> result =
            await Execute(new MoveDepartmentCommand(nonExistingId, new MoveDepartmentRequest(null)));

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentParentNotFoundShouldFailed()
    {
        // arrange
        LocationId? locationId = await CreateLocation("location 1");

        Department? company = await CreateParentDepartment("компания it", "company", [locationId]);

        Department? dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var nonExistingParentId = Guid.NewGuid();

        // act
        Result<Guid, Errors> result = await Execute(
            new MoveDepartmentCommand(dev.Id.Value, new MoveDepartmentRequest(nonExistingParentId)));

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartmentWithoutParentShouldSucceed()
    {
        // arrange
        LocationId? locationId = await CreateLocation("Location 1");

        Department? company = await CreateParentDepartment("компания it", "company", [locationId]);

        Department? dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        Department? fronted = await CreateChildDepartment("фротендеры", "frontend", dev, [locationId]);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await Execute(new MoveDepartmentCommand(dev.Id.Value, new MoveDepartmentRequest(null)));

        // assert
        await ExecuteInDb(async dbContext =>
        {
            Department? departmentDev = await dbContext.Departments.FirstAsync(d => d.Id == dev.Id, cancellationToken);

            Assert.NotNull(departmentDev);
            Assert.Equal("dev", departmentDev.Path.Value);
            Assert.Equal(0, departmentDev.Path.Depth);
            Assert.Null(departmentDev.ParentId);

            Department? departmentFronted =
                await dbContext.Departments.FirstAsync(d => d.Id == fronted.Id, cancellationToken);

            Assert.NotNull(departmentFronted);
            Assert.Equal("dev.frontend", departmentFronted.Path.Value);
            Assert.Equal(1, departmentFronted.Path.Depth);
            Assert.Equal(dev.Id, departmentFronted.ParentId);
        });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task MoveDepartmentWithValidDataShouldSucceed()
    {
        // arrange
        LocationId? locationId = await CreateLocation("Location 1");

        Department? company = await CreateParentDepartment("компания it", "company", [locationId]);

        Department? dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        Department? fronted = await CreateChildDepartment("фронтендеры", "fronted", dev, [locationId]);

        Department? backend = await CreateChildDepartment("бекендеры", "backend", dev, [locationId]);

        Department? team = await CreateChildDepartment("команда 1", "team", fronted, [locationId]);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result = await Execute(
            new MoveDepartmentCommand(fronted.Id.Value, new MoveDepartmentRequest(company.Id.Value)));

        // assert
        await ExecuteInDb(async dbContext =>
        {
            Department? departmentFronted =
                await dbContext.Departments.FirstAsync(d => d.Id == fronted.Id, cancellationToken);

            Assert.NotNull(departmentFronted);
            Assert.Equal("company.fronted", departmentFronted.Path.Value);
            Assert.Equal(1, departmentFronted.Path.Depth);
            Assert.Equal(company.Id.Value, departmentFronted.ParentId!.Value);

            Department? departmentTeam =
                await dbContext.Departments.FirstAsync(d => d.Id == team.Id, cancellationToken);

            Assert.NotNull(departmentTeam);
            Assert.Equal("company.fronted.team", departmentTeam.Path.Value);
            Assert.Equal(2, departmentTeam.Path.Depth);
            Assert.Equal(departmentFronted.Id.Value, departmentTeam.ParentId!.Value);
        });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    private Task<Result<Guid, Errors>> Execute(MoveDepartmentCommand command)
        => Execute<Result<Guid, Errors>, MoveDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}