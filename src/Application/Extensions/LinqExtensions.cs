using System.Linq.Expressions;

namespace Application.Extensions;

public static class LinqExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
    {
        if (!condition)
            return source;

        return source.Where(predicate);
    }
}
