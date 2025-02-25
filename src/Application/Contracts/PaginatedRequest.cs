namespace Application.Contracts;

public record PaginatedRequest()
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}