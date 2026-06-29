using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> EagerLoad<T>(
            this IQueryable<T> query,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes == null)
                return query;

            foreach (var include in includes)
                query = query.Include(include);

            return query;
        }
    }
}
