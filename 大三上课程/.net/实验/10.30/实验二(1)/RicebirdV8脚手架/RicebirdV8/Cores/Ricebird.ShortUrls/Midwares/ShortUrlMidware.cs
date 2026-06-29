using Microsoft.Net.Http.Headers;
using Ricebird.Framework.AspNetCoreExtensions;
using Ricebird.Framework.ShortUrl;

namespace Ricebird.ShortUrls.Midwares
{
    internal class ShortUrlMidware(RequestDelegate next) : RicebirdMidware(next)
    {
        public override async Task Invoke(HttpContext context)
        {
            var urlService = context.RequestServices.Resolve<IShortUrlService>();
            PathString path = context.Request.Path;
            if (!path.StartsWithSegments("/r"))
            {
                await _next(context);
                return;
            }

            // 如果是由/r开始的，那么就执行这里的代码
            string redirectCode = path.ToString().Replace("/r/", "");
            if (redirectCode.Contains('/') || redirectCode.Length < 5)
            {
                await _next(context);
                return;
            }

            string longUrl = urlService.ToLongUrl(redirectCode);
            if (!longUrl.HasValue())
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers[HeaderNames.Location] = "/404";
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers[HeaderNames.Location] = longUrl;
            }
        }
    }
}
