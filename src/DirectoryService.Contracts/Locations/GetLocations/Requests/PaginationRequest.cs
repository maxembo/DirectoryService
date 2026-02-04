namespace DirectoryService.Contracts.Locations.GetLocations.Requests;

public record PaginationRequest(int Page = 1, int PageSize = 20);