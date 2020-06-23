using System;
using System.Linq;

namespace temp_tracker.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> Paged<T>(this IQueryable<T> query, int count, int? page, int? limit)
        {
            int skip = Math.Max(((page ?? 1) - 1) * (limit ?? 0), 0);
            int take = Math.Max((limit ?? count), 0);

            return query
                .Skip(skip)
                .Take(take);
        } 
    }
}