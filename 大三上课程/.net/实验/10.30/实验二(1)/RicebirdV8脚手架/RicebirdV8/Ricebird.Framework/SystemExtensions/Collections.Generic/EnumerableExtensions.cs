namespace System.Collections.Generic
{
    /// <summary> 
    /// <see cref="IEnumerable{T}"/> 的扩展方法。
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 将 <see cref="IEnumerable{T}"/> 中的每个元素使用 separator 连接起来.
        /// 即是 string.Join(...) 的简写
        /// </summary>
        /// <param name="source">待连接字符串组.</param>
        /// <param name="separator">连接符</param>
        /// <returns>返回连接结果，如果source中没有任何内容，则返回string.Empty.</returns>
        public static string JoinAsString(this IEnumerable<string> source, char separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 将 <see cref="IDictionary{String, TValue}"/> 中的每个元素的Key使用 separator 连接起来.
        /// 即是 string.Join(...) 的简写
        /// </summary>
        /// <param name="source">待连接字典.</param>
        /// <param name="separator">连接符</param>
        /// <returns>返回连接结果，如果source中没有任何内容，则返回string.Empty.</returns>
        public static string JoinAsString<TValue>(this IDictionary<string, TValue> source, string separator)
        {
            return string.Join(separator, source.Keys);
        }

        /// <summary>
        /// 将 <see cref="IEnumerable{T}"/> 中的每个元素使用 separator 连接起来.
        /// 即是 string.Join(...) 的简写
        /// </summary>
        /// <param name="source">待连接字符串组.</param>
        /// <param name="separator">连接符</param>
        /// <returns>返回连接结果，如果source中没有任何内容，则返回string.Empty.</returns>
        public static string JoinAsString(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 将 <see cref="IEnumerable{T}"/> 中的每个元素使用 separator 连接起来.
        /// 即是 string.Join(...) 的简写
        /// </summary>
        /// <param name="source">待连接对象组.</param>
        /// <param name="separator">The string to use as a separator. separator is included in the returned string only if values has more than one element.</param>
        /// <param name="separator">连接符</param>
        /// <returns>返回连接结果，如果source中没有任何内容，则返回string.Empty.</returns>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 如果满足指定条件，则在 <see cref="IEnumerable{T}"/> 中进行查询，反之不作任何操作.
        /// </summary>
        /// <param name="source">源查询</param>
        /// <param name="condition">指定条件</param>
        /// <param name="predicate">需要附加的查询</param>
        /// <returns>基于 <paramref name="condition"/> 的值返回查询 </returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, OnOffValue condition, Func<T, bool> predicate)
        {
            return condition
                ? source.Where(predicate)
                : source;
        }

        /// <summary>
        /// 如果满足指定条件，则在 <see cref="IEnumerable{T}"/> 中进行查询，反之不作任何操作.
        /// </summary>
        /// <param name="source">源查询</param>
        /// <param name="condition">指定条件</param>
        /// <param name="predicate">需要附加的查询，第二个参数为index，即每个元素的序号</param>
        /// <returns>基于 <paramref name="condition"/> 的值返回查询 </returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, OnOffValue condition, Func<T, int, bool> predicate)
        {
            return condition
                ? source.Where(predicate)
                : source;
        }
    }
}
