using Domain.Aggregates;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedAt).IsRequired(true);
        builder.Property(entity => entity.UpdatedAt).IsRequired(false);
        builder.Property(entity => entity.CanHaveEntries).IsRequired(true);
        builder.Property(entity => entity.Name).IsRequired(true).HasMaxLength(100);
        builder.Property(entity => entity.Description).IsRequired(false).HasMaxLength(250);

        builder.HasOne(entity => entity.AccountType);

        builder.HasOne(entity => entity.Parent)
            .WithMany(parent => parent.ChildAccounts);

        builder.Property(entity => entity.Code)
            .HasConversion(
                c => c.ToString(),
                c => new CodeVo(c))
            .IsRequired(true);
    }
}
