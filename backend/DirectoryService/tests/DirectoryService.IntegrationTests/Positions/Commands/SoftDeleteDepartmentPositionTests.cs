using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class SoftDeleteDepartmentPositionTests(DirectoryTestWebFactory factory) : PositionBaseTests(factory)
{
    [Fact]
    public async Task SoftDeleteDepartment_WhenPositionHasNoOtherActiveDepartment_ShouldDeactivatePosition()
    {
        // arrange
        var department = await SeedActiveDepartment();

        var positionId = await CreatePosition(
            "Position without another department",
            "description",
            [department.Id]);

        var command = new SoftDeleteDepartmentCommand(department.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(department.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .SingleAsync(currentDepartment => currentDepartment.Id == department.Id);

            Assert.False(deletedDepartment.IsActive);
            Assert.NotNull(deletedDepartment.DeletedAt);

            var position = await dbContext.Positions
                .Include(currentPosition => currentPosition.Departments)
                .SingleAsync(currentPosition => currentPosition.Id == positionId);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);

            var relation = Assert.Single(position.Departments);
            Assert.Equal(department.Id, relation.DepartmentId);
            Assert.Equal(positionId, relation.PositionId);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenPositionHasAnotherActiveDepartment_ShouldKeepPositionActive()
    {
        // arrange
        var departmentToDelete = await SeedActiveDepartment("deleted", "deleted-department");
        var remainingDepartment = await SeedActiveDepartment("remaining", "remaining-department");

        var positionId = await CreatePosition(
            "Position with another active department",
            "description",
            [departmentToDelete.Id, remainingDepartment.Id]);

        var command = new SoftDeleteDepartmentCommand(departmentToDelete.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentToDelete.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .SingleAsync(department => department.Id == departmentToDelete.Id);

            var activeDepartment = await dbContext.Departments
                .SingleAsync(department => department.Id == remainingDepartment.Id);

            Assert.False(deletedDepartment.IsActive);
            Assert.NotNull(deletedDepartment.DeletedAt);
            Assert.True(activeDepartment.IsActive);
            Assert.Null(activeDepartment.DeletedAt);

            var position = await dbContext.Positions
                .Include(currentPosition => currentPosition.Departments)
                .SingleAsync(currentPosition => currentPosition.Id == positionId);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
            Assert.Equal(2, position.Departments.Count);

            Assert.Contains(
                position.Departments,
                relation => relation.DepartmentId == departmentToDelete.Id);

            Assert.Contains(
                position.Departments,
                relation => relation.DepartmentId == remainingDepartment.Id);
        });
    }

    private Task<Result<Guid, Errors>> Execute(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(
            handler => handler.Handle(command, CancellationToken.None));
}
