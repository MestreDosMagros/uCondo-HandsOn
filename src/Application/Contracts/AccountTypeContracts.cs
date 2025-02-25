namespace Application.Contracts;

public record GetAccountTypeRequest(int Id);
public record DeleteAccountTypeRequest(int Id);
public record CreateAccountTypeRequest(string Name, string Description);
public record UpdateAccountTypeRequest(string? Name, string? Description);
public record ListAccountTypesRequest(int? Id, string? Name, string? Description) : PaginatedRequest;

public record GetAccountTypeResponse(int Id, string Name, string? Description);
public record CreateAccountTypeResponse(int Id, string Name, string? Description);
public record UpdateAccountTypeResponse(int Id, string Name, string? Description);
public record ListAccountTypesResponseItem(int Id, string Name, string? Description);