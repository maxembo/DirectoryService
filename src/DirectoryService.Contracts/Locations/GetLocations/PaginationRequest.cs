namespace DirectoryService.Contracts.Locations.GetLocations;

public record PaginationRequest(int Page = 1, int PageSize = 20);