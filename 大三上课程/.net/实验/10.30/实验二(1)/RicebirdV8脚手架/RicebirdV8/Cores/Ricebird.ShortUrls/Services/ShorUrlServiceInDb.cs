using Ricebird.Framework.ShortUrl;
using Ricebird.ShortUrls.Models;

namespace Ricebird.ShortUrls.Services
{
    public class ShortUrlServiceInDb : IShortUrlService
    {
        private IServiceProvider Services { get; init; }
        internal static object lck = new object();
        public MemoryCache ShortUrlCache
        {
            get; set;
        }

        public ShortUrlServiceInDb(IServiceProvider service)
        {
            ShortUrlCache = new MemoryCache(new MemoryCacheOptions());
            Services = service;
            using var scope = Services.CreateScope();
            UrlRepository repo = scope.Resolve<UrlRepository>();
            repo.ClearExpiredUrl();
            var links = repo.GetUrlLinks();
            foreach (var item in links)
            {
                AddEntry(item);
            }
        }

        private void AddEntry(UrlLink item)
        {
            ShortUrlCache.Remove(item.ShortUrl);
            using var entry = ShortUrlCache.CreateEntry(item.ShortUrl);
            entry.Value = item;
            entry.SetAbsoluteExpiration(item.RemoveTime);
        }

        /// <summary>
        /// 添加一次性的映射
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ToShortUrlCode(string url)
        {
            string code = GenerateId(6);
            var link = new UrlLink()
            {
                ShortUrl = code,
                LongUrl = url,
                AddTime = DateTime.Now,
                RemoveTime = DateTime.Now.AddSeconds(60),
                LinkState = UrlState.Once
            };
            AddEntry(link);
            return link.ShortUrl;
        }

        /// <summary>
        /// 是回临时的映射
        /// </summary>
        /// <param name="url"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string ToShortUrlCode(IClient client, string url, TimeSpan duration)
        {
            var repo = client.Resolve<UrlRepository>();
            UrlLink link = repo.GetUrlLink(url, duration, UrlState.Temporary);
            AddEntry(link);
            return link.ShortUrl;
        }

        /// <summary>
        /// 是回永久的映射
        /// </summary>
        /// <param name="url"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string ToPermanentUrlCode(IClient client, string url)
        {
            var repo = client.Resolve<UrlRepository>();
            UrlLink link = repo.GetUrlLink(url, TimeSpan.FromDays(18250), UrlState.Permanent);
            AddEntry(link);
            return link.ShortUrl;
        }

        public string ToShortUrl(IClient client, string url)
        {
            string code = ToShortUrlCode(url);
            return $"{client.HostWithScheme}/r/{code}";
        }

        public string ToShortUrl(IClient client, string url, TimeSpan duration)
        {
            string code = ToShortUrlCode(client, url, duration);
            return $"{client.HostWithScheme}/r/{code}";
        }

        public string ToPermanentUrl(IClient client, string url)
        {
            string code = ToPermanentUrlCode(client, url);
            return $"{client.HostWithScheme}/r/{code}";
        }

        public string ToLongUrl(string code)
        {
            if (ShortUrlCache.TryGetValue(code, out object? linkEntry) && linkEntry is UrlLink link)
            {
                // 过期的，不返回
                if (link.IsExpired)
                {
                    ShortUrlCache.Remove(link.ShortUrl);
                    return "";
                }

                // 一次性的不返回
                if (link.LinkState == UrlState.Once)
                {
                    ShortUrlCache.Remove(link.ShortUrl);
                }

                return link.LongUrl;
            }

            return "";
        }
    }
}
