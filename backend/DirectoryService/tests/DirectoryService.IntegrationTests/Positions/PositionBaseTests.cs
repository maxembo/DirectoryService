using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions;

public class PositionBaseTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    protected static void AssertSingleError(Result<Guid, Errors> result, Error expectedError)
    {
        Assert.True(result.IsFailure);

        Error? actualError = Assert.Single(result.Error);

        Assert.Equal(expectedError.Code, actualError.Code);
        Assert.Equal(expectedError.Type, actualError.Type);
        Assert.Equal(expectedError.InvalidField, actualError.InvalidField);
    }

    protected async Task<Department> SeedActiveDepartment(
        string suffix = "default",
        string departmentIdentifier = "department-default")
    {
        LocationId? locationId = await CreateLocation(
            $"test location {suffix}",
            suffix,
            suffix,
            suffix);

        return await CreateParentDepartment(
            $"test department {suffix}",
            departmentIdentifier,
            [locationId]);
    }

    protected Task AssertPositionTableCounts(
        int expectedPositionCount,
        int expectedDepartmentPositionCount)
    {
        return ExecuteInDb(async dbContext =>
        {
            int positionCount =
                await dbContext.Positions.CountAsync();

            int departmentPositionCount =
                await dbContext.DepartmentPositions.CountAsync();

            Assert.Equal(expectedPositionCount, positionCount);
            Assert.Equal(
                expectedDepartmentPositionCount,
                departmentPositionCount);
        });
    }
}