namespace DirectoryService.Contracts;

public abstract record PaginationRequest(int Page = 1, int PageSize = 20);