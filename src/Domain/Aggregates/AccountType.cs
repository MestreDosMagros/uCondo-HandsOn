namespace Domain.Aggregates;

public sealed class AccountType
{
    public AccountType()
    {
    }

    public AccountType(string name, string description)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
