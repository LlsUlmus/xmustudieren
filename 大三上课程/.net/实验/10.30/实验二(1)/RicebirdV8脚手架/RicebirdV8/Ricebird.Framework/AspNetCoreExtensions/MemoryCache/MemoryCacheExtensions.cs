namespace Microsoft.Extensions.Caching.Memory;

public static class MemoryCacheExtensions
{
    public static void SetSlider(this IMemoryCache cache, object key, object value, TimeSpan sliderTimeSpan)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = value;
        entry.SetSlidingExpiration(sliderTimeSpan);
    }

    public static void Set<TEntity>(this IMemoryCache cache, object key, Func<ICacheEntry, TEntity> option)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = option(entry);
    }

    public static void SetSlider(this MemoryCache cache, object key, object value, TimeSpan sliderTimeSpan)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = value;
        entry.SetSlidingExpiration(sliderTimeSpan);
    }

    public static void Set<TEntity>(this MemoryCache cache, object key, Func<ICacheEntry, TEntity> option)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = option(entry);
    }
}

