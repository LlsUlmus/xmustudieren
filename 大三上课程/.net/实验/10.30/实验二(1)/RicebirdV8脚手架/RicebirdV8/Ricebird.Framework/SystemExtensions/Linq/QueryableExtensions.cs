using System.Linq.Expressions;

namespace System.Linq
{
    /// <summary>
    /// Some useful extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// 分页
        /// </summary>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int page, int pageSize, out int totalRow)
        {
            totalRow = query.Count();
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 分页
        /// </summary>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 如果满足指定条件，则在 <see cref="IEnumerable{T}"/> 中进行查询，反之不作任何操作.
        /// </summary>
        /// <param name="source">源查询</param>
        /// <param name="condition">指定条件</param>
        /// <param name="predicate">需要附加的查询</param>
        /// <returns>基于 <paramref name="condition"/> 的值返回查询 </returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, OnOffValue condition, Expression<Func<T, bool>> predicate)
        {
            return condition
                ? source.Where(predicate)
                : source;
        }

        /// <summary>
        /// Where和ToList的整合函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return query.Where(predicate).ToList();
        }

        /// <summary>
        /// Where，OrderBy和ToList的整合函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T> ToOrderByList<T, TKey>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector)
        {
            return query.Where(predicate).OrderBy(keySelector).ToList();
        }

        /// <summary>
        /// Where，OrderBy和ToList的整合函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T> ToOrderByDescendingList<T, TKey>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector)
        {
            return query.Where(predicate).OrderByDescending(keySelector).ToList();
        }
    }
}
