using Ricebird.Framework.ShortUrl;

namespace Ricebird.ShortUrls.Models
{
    public class UrlRepository : RepositoryBase<UrlLink>
    {
        public UrlRepository(RicebirdContext ctx, IServiceProvider scoped) : base(ctx, scoped)
        {
        }

        public void ClearExpiredUrl()
        {
            DbSet.Where(e => e.RemoveTime < DateTime.Now || e.LinkState == UrlState.Once).ExecuteDelete();
        }

        public List<UrlLink> GetUrlLinks()
        {
            return DbSet.ToList();
        }

        /// <summary>
        /// 根据长名称和生效时长，获取一个链接
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public UrlLink GetUrlLink(string longUrl, TimeSpan duration, UrlState linkState)
        {
            DateTime expiredOn = DateTime.Now + duration;

            if (duration.TotalMinutes < 5 || linkState == UrlState.Once)
            {
                // 5分钟以下或者一次性的不进数据库，直接缓存里生成一个
                return new UrlLink()
                {
                    LongUrl = longUrl,
                    ShortUrl = GenerateId(6),
                    AddTime = DateTime.Now,
                    RemoveTime = expiredOn,
                    LinkState = linkState,
                };
            }

            var link = DbSet.FirstOrDefault(e => e.LongUrl == longUrl && e.LinkState == linkState);
            if (link == null)
            {
                link = new UrlLink()
                {
                    LongUrl = longUrl,
                    ShortUrl = GenerateId(6),
                    AddTime = DateTime.Now,
                    RemoveTime = expiredOn,
                    LinkState = linkState,
                };

                DbSet.Add(link);
            }
            else
            {
                link.RemoveTime = expiredOn;
            }
            SaveChanges();
            return link;
        }
    }
}
