using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<Context>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), cfg =>
            {
                cfg.MigrationsAssembly(typeof(Context).Assembly.GetName().Name);
            });
        });

        return services;
    }
}
