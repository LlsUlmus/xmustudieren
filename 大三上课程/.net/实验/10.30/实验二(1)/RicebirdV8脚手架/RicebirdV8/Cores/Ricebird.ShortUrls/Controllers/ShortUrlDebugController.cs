using Ricebird.Framework.ShortUrl;

namespace Ricebird.ShortUrls.Controllers
{
    [Route("~/debug/url/[action]")]
    public class ShortUrlDebugController(IShortUrlService urlService) : DebugController
    {
        public ActionResult ToShort()
        {
            var url = urlService.ToShortUrl(Client, "https://www.baidu.com", TimeSpan.FromMinutes(6));
            return Ok(url);
        }

    }
}
