namespace Shared.Response;

public record PaginationEnvelope<T>
{
    public T[] Items { get; init; } = [];

    public long TotalCount { get; init; }

    public PaginationEnvelope(IEnumerable<T> items, long totalCount)
    {
        Items = items.ToArray();
        TotalCount = totalCount;
    }
}