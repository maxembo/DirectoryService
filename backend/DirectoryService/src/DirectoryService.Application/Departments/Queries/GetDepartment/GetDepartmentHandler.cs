using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Domain.Departments;
using SharedService.Core.Abstractions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Queries.GetDepartment;

public class GetDepartmentHandler : IQueryHandler<GetDepartmentDetailDto, GetDepartmentQuery>
{
    private readonly IDepartmentsRepository _departmentsRepository;

    public GetDepartmentHandler(IDepartmentsRepository departmentsRepository)
    {
        _departmentsRepository = departmentsRepository;
    }

    public async Task<Result<GetDepartmentDetailDto, Errors>> Handle(
        GetDepartmentQuery query, CancellationToken cancellationToken)
    {
        var departmentId = DepartmentId.Create(query.DepartmentId);

        var departmentResult = await _departmentsRepository.GetBy(d => d.Id == departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        return new GetDepartmentDetailDto()
        {
            Id = department.Id.Value,
            Name = department.Name.Value,
            Identifier = department.Identifier.Value,
            ParentId = department.ParentId?.Value,
            Path = department.Path.Value,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            DeletedAt = department.DeletedAt,
        };
    }
}