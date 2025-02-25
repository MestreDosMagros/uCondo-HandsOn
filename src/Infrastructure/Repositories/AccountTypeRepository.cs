using Domain.Aggregates;

namespace Infrastructure.Repositories;

public sealed class AccountTypeRepository : Repository<AccountType>, IAccountTypeRepository
{
    public AccountTypeRepository(Context context) : base(context)
    {
    }
}
