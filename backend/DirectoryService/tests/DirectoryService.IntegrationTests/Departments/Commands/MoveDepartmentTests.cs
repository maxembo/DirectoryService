using System.Net;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.MoveDepartments;
using DirectoryService.Contracts.Departments.MoveDepartments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class MoveDepartmentTests : DirectoryBaseTests
{
    private readonly HttpClient _client;

    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MoveDepartment_WhenParentIsArchived_ShouldReturnParentDeletedError()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        await MarkDepartmentAsDeleted(company.Id);

        var backend = await CreateParentDepartment("backend", "backend", [locationId]);

        var command = CreateCommand(backend.Id.Value, company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            error => error is
            {
                Code: "department.move.parent_deleted", Type: ErrorType.CONFLICT, InvalidField: "department.parentId",
            });
    }

    [Fact]
    public async Task MoveDepartment_WhenTargetIsSelf_ShouldReturnCycleError()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var command = CreateCommand(company.Id.Value, company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            error => error is
            {
                Code: "department.move.cycle",
                InvalidField: "department.parentId",
            });
    }

    [Fact]
    public async Task MoveDepartment_WhenTargetIsDescendant_ShouldReturnCycleError()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var command = CreateCommand(company.Id.Value, dev.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            error => error is
            {
                Code: "department.move.cycle",
                InvalidField: "department.parentId",
            });
    }

    [Fact]
    public async Task MoveDepartment_WhenDepartmentDoesNotExist_ShouldReturnNotFoundError()
    {
        // arrange
        var nonExistingId = Guid.NewGuid();

        var command = CreateCommand(nonExistingId, null);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            error => error is
            {
                Code: "value.not.found",
                Type: ErrorType.NOT_FOUND,
                InvalidField: null,
            });
    }

    [Fact]
    public async Task MoveDepartment_WhenParentDoesNotExist_ShouldReturnNotFoundError()
    {
        // arrange
        var locationId = await CreateLocation("location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var nonExistingParentId = Guid.NewGuid();

        var command = CreateCommand(dev.Id.Value, nonExistingParentId);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            error => error is
            {
                Code: "department.move.parent_not_found",
                Type: ErrorType.NOT_FOUND,
                InvalidField: "department.parentId",
            });
    }

    [Fact]
    public async Task MoveDepartment_WhenParentIsNull_ShouldMoveSubtreeToRoot()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var fronted = await CreateChildDepartment("фротендеры", "frontend", dev, [locationId]);

        var cancellationToken = CancellationToken.None;

        var command = CreateCommand(dev.Id.Value, null);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var departmentDev = await dbContext.Departments.FirstAsync(d => d.Id == dev.Id, cancellationToken);

            Assert.NotNull(departmentDev);
            Assert.Equal("dev", departmentDev.Path.Value);
            Assert.Equal(0, departmentDev.Path.Depth);
            Assert.Null(departmentDev.ParentId);

            var departmentFronted =
                await dbContext.Departments.FirstAsync(d => d.Id == fronted.Id, cancellationToken);

            Assert.NotNull(departmentFronted);
            Assert.Equal("dev.frontend", departmentFronted.Path.Value);
            Assert.Equal(1, departmentFronted.Path.Depth);
            Assert.Equal(dev.Id, departmentFronted.ParentId);
        });

        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task MoveDepartment_WhenTargetIsValid_ShouldMoveSubtree()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var company = await CreateParentDepartment("компания it", "company", [locationId]);

        var dev = await CreateChildDepartment("разработка", "dev", company, [locationId]);

        var fronted = await CreateChildDepartment("фронтендеры", "fronted", dev, [locationId]);

        var backend = await CreateChildDepartment("бекендеры", "backend", dev, [locationId]);

        var team = await CreateChildDepartment("команда 1", "team", fronted, [locationId]);

        var cancellationToken = CancellationToken.None;

        var command = CreateCommand(fronted.Id.Value, company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var departmentFronted =
                await dbContext.Departments.FirstAsync(d => d.Id == fronted.Id, cancellationToken);

            Assert.NotNull(departmentFronted);
            Assert.Equal("company.fronted", departmentFronted.Path.Value);
            Assert.Equal(1, departmentFronted.Path.Depth);
            Assert.Equal(company.Id.Value, departmentFronted.ParentId!.Value);

            var departmentTeam =
                await dbContext.Departments.FirstAsync(d => d.Id == team.Id, cancellationToken);

            Assert.NotNull(departmentTeam);
            Assert.Equal("company.fronted.team", departmentTeam.Path.Value);
            Assert.Equal(2, departmentTeam.Path.Depth);
            Assert.Equal(departmentFronted.Id.Value, departmentTeam.ParentId!.Value);
        });

        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task MoveDepartmentEndpoint_WhenRequestUsesPut_ShouldMoveDepartmentToRoot()
    {
        // arrange
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);
        var development = await CreateChildDepartment("development", "dev", company, [locationId]);

        // act
        using var response = await _client.PutAsJsonAsync(
            $"/api/departments/{development.Id.Value}/parent",
            new MoveDepartmentRequest(null));

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .SingleAsync(department => department.Id == development.Id);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal("dev", movedDepartment.Path.Value);
        });
    }

    private static MoveDepartmentCommand CreateCommand(Guid departmentId, Guid? parentId) =>
        new(departmentId, new MoveDepartmentRequest(parentId));

    private Task<Result<Guid, Errors>> Execute(MoveDepartmentCommand command)
        => Execute<Result<Guid, Errors>, MoveDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}