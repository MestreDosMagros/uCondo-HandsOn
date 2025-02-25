using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public sealed class Account
{
    private List<Account> _ChildAccounts = new();

    public Account()
    {
    }

    public Account(string name, string code, string description, bool canHaveEntries, int? parentId, int accountTypeId)
    {
        Code = code;
        Name = name;
        ParentId = parentId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        AccountTypeId = accountTypeId;
        CanHaveEntries = canHaveEntries;
    }

    public int Id { get; set; }
    public CodeVo Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool CanHaveEntries { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? ParentId { get; set; }
    public Account? Parent { get; set; }

    public int AccountTypeId { get; set; }
    public AccountType? AccountType { get; set; }

    public IReadOnlyCollection<Account> ChildAccounts => _ChildAccounts.AsReadOnly();

    public void Update(string? code, string? name, string? description, bool? canHaveEntries, int? accountTypeId, int? parentId)
    {
        Name = name ?? Name;
        ParentId = parentId;
        Code = code ?? Code;
        UpdatedAt = DateTime.UtcNow;
        Description = description ?? Description;
        AccountTypeId = accountTypeId ?? AccountTypeId;
        CanHaveEntries = canHaveEntries ?? CanHaveEntries;
    }

    public void AddChild(Account account)
    {
        if (account.AccountTypeId != AccountTypeId)
        {
            throw new DomainException("Account can't have different type from parent account");
        }

        _ChildAccounts.Add(account);
    }
}
