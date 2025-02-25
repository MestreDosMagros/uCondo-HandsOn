using Domain.Aggregates;

namespace Infrastructure.Repositories;

public sealed class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(Context context) : base(context)
    {
    }
}
