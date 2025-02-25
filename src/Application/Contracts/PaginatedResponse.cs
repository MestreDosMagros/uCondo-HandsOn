namespace Application.Contracts;

public record PaginatedResponse<T>(IList<T> Items, int Page, int PageSize);