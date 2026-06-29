using Ricebird.Framework.ShortUrl;

namespace Ricebird.ShortUrls.Models
{
    public class UrlLink : EntityBase
    {
        public string ShortUrl { get; set; } = string.Empty;

        public string LongUrl { get; set; } = string.Empty;

        public DateTime AddTime { get; set; } = DateTime.Now;

        public DateTime RemoveTime { get; set; } = DateTime.Now;

        public UrlState LinkState { get; set; } = UrlState.Once;

        [NotMapped]
        public bool IsExpired => DateTime.Now > RemoveTime;
    }
}
