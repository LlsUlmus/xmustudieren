using System.Collections.Concurrent;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for Collections.
    /// </summary>
    public static class CollectionExtensions
    {
        public static TValue? GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (dic.TryGetValue(key, out TValue? value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 合并键
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static IDictionary<TKey, TValue> MergeKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue? value)
            where TKey : notnull
        {
            if (value == null)
            {
                return dic;
            }

            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }

            return dic;
        }

        /// <summary>
        /// 合并键
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static ConcurrentDictionary<TKey, TValue> MergeKey<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic, TKey key, TValue? value)
            where TKey : notnull
        {
            if (value == null)
            {
                return dic;
            }

            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.TryAdd(key, value);
            }

            return dic;
        }

        /// <summary>
        /// 合并列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDictionary<TKey, List<TValue>> MergeList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dic, TKey key, TValue? value)
        {
            if (value == null)
            {
                return dic;
            }

            if (dic.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                dic.Add(key, [value]);
            }

            return dic;
        }

        /// <summary>
        /// 合并到第一个字典中
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="mergeSource"></param>
        public static IDictionary<TKey, TValue> MergeDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dic, params IDictionary<TKey, TValue>[] mergeSource)
            where TKey : notnull
        {
            foreach (var mDic in mergeSource)
            {
                foreach (var kv in mDic)
                {
                    dic.MergeKey(kv.Key, kv.Value);
                }
            }

            return dic;
        }

        /// <summary>
        /// 合并到一个新字典中
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="mergeSource"></param>
        public static IDictionary<TKey, TValue> MergeToDictionary<TKey, TValue>(params IDictionary<TKey, TValue>[] mergeSource)
            where TKey : notnull
        {
            Dictionary<TKey, TValue> dic = [];
            foreach (var mDic in mergeSource)
            {
                foreach (var kv in mDic)
                {
                    dic.MergeKey(kv.Key, kv.Value);
                }
            }

            return dic;
        }

        public static FrozenDictionary<TValue, TKey> ReverseKeyValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : notnull
            where TValue : notnull
        {
            Dictionary<TValue, TKey> reverse = [];
            foreach (var kv in dict)
            {
                reverse.MergeKey(kv.Value, kv.Key);
            }

            return reverse.ToFrozenDictionary();
        }
    }
}