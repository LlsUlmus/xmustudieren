using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Ricebird.Cms.DataSources
{
    public class PageCachePolicy : IOutputCachePolicy, ISingletonDependency
    {
        private MemoryCache Cache { get; set; } = new MemoryCache(new MemoryCacheOptions());

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            context.EnableOutputCaching = true;
            context.AllowCacheStorage = true;

            IClient client = context.HttpContext.Resolve<IClient>();
            string path = context.HttpContext.Request.GetDisplayUrl();
            // context.AllowCacheLookup = !GetCache(path, out _);
            if (!GetCache(path, out string? cacheValue))
            {
                context.AllowCacheLookup = false;
            }
            else
            {
                context.AllowCacheLookup = true;
                Console.WriteLine($"从缓存中载入{path}");
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            IClient client = context.HttpContext.Resolve<IClient>();
            string path = context.HttpContext.Request.GetDisplayUrl();
            SetCache(path, client);
            Console.WriteLine($"重新构建了{path}");
            return ValueTask.CompletedTask;
        }

        public bool GetCache(string cacheKey, [NotNullWhen(true)] out string? html) => Cache.TryGetValue(cacheKey, out html);

        public void SetCache(string cacheKey, IClient client)
        {
            PageBuilder builder = client.Resolve<PageBuilder>();
            IChangeToken changeToken = builder.BuildChangeToken();
            Cache.Set(cacheKey, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromSeconds(3600)); // 滑动过时 3600秒
                entry.AddExpirationToken(changeToken);
                return cacheKey;
            });
        }
    }
}
