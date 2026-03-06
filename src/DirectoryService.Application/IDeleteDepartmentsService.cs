using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Application;

public interface IDeleteDepartmentsService
{
    public Task<UnitResult<Error>> Process(CancellationToken cancelToken = default);
}