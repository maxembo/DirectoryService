using System.Net;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.ChangeDepartmentActivity;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Contracts.Departments.ChangeActivity;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class ChangeDepartmentActivityTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ChangeActivity_WhenDepartmentIsActive_ShouldDeactivateWithoutArchiving()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);
        var command = new ChangeDepartmentActivityCommand(company.Id.Value, false);

        var result = await Execute(command);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);

            Assert.False(department.IsActive);
            Assert.Null(department.DeletedAt);
        });
    }

    [Fact]
    public async Task ChangeActivity_WhenDepartmentIsInactive_ShouldActivate()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);
            department.SetActivity(false);
            await dbContext.SaveChangesAsync();
        });

        var command = new ChangeDepartmentActivityCommand(company.Id.Value, true);

        var result = await Execute(command);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);

            Assert.True(department.IsActive);
            Assert.Null(department.DeletedAt);
        });
    }

    [Fact]
    public async Task ChangeActivity_WhenDepartmentIsArchived_ShouldFailWithoutRestoring()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);
        await MarkDepartmentAsDeleted(company.Id);

        var command = new ChangeDepartmentActivityCommand(company.Id.Value, true);

        var result = await Execute(command);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.activity.archived");

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);

            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);
        });
    }

    [Fact]
    public async Task ChangeActivity_WhenDepartmentHasActiveDescendant_ShouldNotDeactivate()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);
        await CreateChildDepartment("development", "development", company, [locationId]);

        var command = new ChangeDepartmentActivityCommand(company.Id.Value, false);

        var result = await Execute(command);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.activity.active_descendants");

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);
            Assert.True(department.IsActive);
        });
    }

    [Fact]
    public async Task ChangeActivity_WhenActiveDescendantIsBehindArchivedDepartment_ShouldDeactivate()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);
        var archived = await CreateChildDepartment("archived", "archived", company, [locationId]);

        await CreateChildDepartment("active", "active", archived, [locationId]);

        var archiveResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(archived.Id.Value));

        Assert.True(archiveResult.IsSuccess);

        // act
        var result = await Execute(new ChangeDepartmentActivityCommand(company.Id.Value, false));

        Assert.True(result.IsSuccess);
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);
            Assert.False(department.IsActive);
        });
    }

    [Fact]
    public async Task ChangeActivity_WhenParentIsInactive_ShouldNotActivateChild()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);
        var development = await CreateChildDepartment("development", "development", company, [locationId]);

        await ExecuteInDb(async dbContext =>
        {
            var departments = await dbContext.Departments
                .Where(d => d.Id == company.Id || d.Id == development.Id)
                .ToListAsync();

            foreach (var department in departments)
            {
                department.SetActivity(false);
            }

            await dbContext.SaveChangesAsync();
        });

        var command = new ChangeDepartmentActivityCommand(development.Id.Value, true);

        var result = await Execute(command);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.activity.inactive_parent");

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == development.Id);
            Assert.False(department.IsActive);
        });
    }

    [Fact]
    public async Task ChangeActivityEndpoint_WhenRequestIsValid_ShouldSetRequestedStatus()
    {
        var locationId = await CreateLocation();
        var company = await CreateParentDepartment("company", "company", [locationId]);

        using var response = await _client.PatchAsJsonAsync(
            $"/api/departments/{company.Id.Value}/activity",
            new ChangeDepartmentActivityRequest(false));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == company.Id);
            Assert.False(department.IsActive);
        });
    }

    private Task<Result<Guid, Errors>> Execute(ChangeDepartmentActivityCommand command) =>
        Execute<Result<Guid, Errors>, ChangeDepartmentActivityHandler>(handler =>
            handler.Handle(command, CancellationToken.None));

    private Task<Result<Guid, Errors>> ExecuteSoftDelete(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler =>
            handler.Handle(command, CancellationToken.None));
}
