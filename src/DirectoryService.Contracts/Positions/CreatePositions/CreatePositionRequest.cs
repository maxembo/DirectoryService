namespace DirectoryService.Contracts.Positions.CreatePositions;

public record CreatePositionRequest(string Name, string? Description, Guid[] DepartmentIds);
