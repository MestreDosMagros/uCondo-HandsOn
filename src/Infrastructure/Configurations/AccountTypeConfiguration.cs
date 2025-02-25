using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedAt).IsRequired(true);
        builder.Property(entity => entity.UpdatedAt).IsRequired(false);
        builder.Property(entity => entity.Name).IsRequired(true).HasMaxLength(50);
        builder.Property(entity => entity.Description).IsRequired(false).HasMaxLength(250);
    }
}
