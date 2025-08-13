using System.Linq.Expressions;
using System.Reflection;

namespace DeveloperStore.Application.Common;

public static class OrderParser
{
    public static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string order)
    {
        // order example: "price desc,title asc"
        var parts = order.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<T>? ordered = null;
        foreach (var (idx, part) in parts.Select((p, i) => (i, p)))
        {
            var tokens = part.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var prop = tokens[0];
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var param = Expression.Parameter(typeof(T), "x");
            var propInfo = typeof(T).GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null) continue;
            var propExpr = Expression.Property(param, propInfo);
            var keySelector = Expression.Lambda(propExpr, param);

            if (idx == 0)
            {
                ordered = desc
                    ? Queryable.OrderByDescending(source, (dynamic)keySelector)
                    : Queryable.OrderBy(source, (dynamic)keySelector);
            }
            else if (ordered != null)
            {
                ordered = desc
                    ? Queryable.ThenByDescending(ordered, (dynamic)keySelector)
                    : Queryable.ThenBy(ordered, (dynamic)keySelector);
            }
        }
        return ordered ?? (IOrderedQueryable<T>)source;
    }
}
