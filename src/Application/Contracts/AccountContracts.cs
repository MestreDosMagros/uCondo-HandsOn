namespace Application.Contracts;

public record GetAccountRequest(int Id);
public record DeleteAccountRequest(int Id);
public record GetNextCodeRequest(int? AccountId);
public record CreateAccountRequest(string Name, string Code, string Description, bool CanHaveEntries, int? ParentId, int? AccountTypeId);
public record UpdateAccountRequest(string? Code, string? Name, string? Description, bool? CanHaveEntries, int? AccountTypeId, int? ParentId);
public record ListAccountsRequest(int? Id, string? Code, string? Name, string? Description, bool? CanHaveEntries, int? ParentId, int? AccountTypeId) : PaginatedRequest;

public record GetNextCodeResponse(string Code);
public record GetAccountResponse(int Id, string Name, string Code, string Description, bool CanHaveEntries, int? ParentId, int? AccountTypeId);
public record CreateAccountResponse(int Id, string Name, string Code, string Description, bool CanHaveEntries, int? ParentId, int? AccountTypeId);
public record UpdateAccountResponse(int Id, string Name, string Code, string Description, bool CanHaveEntries, int? ParentId, int? AccountTypeId);
public record ListAccountsResponseItem(int Id, string Name, string Code, string Description, bool CanHaveEntries, int? ParentId, int? AccountTypeId);
