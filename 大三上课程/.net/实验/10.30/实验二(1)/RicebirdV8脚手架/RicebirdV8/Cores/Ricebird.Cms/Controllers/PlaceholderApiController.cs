using SkiaSharp;

namespace Ricebird.Cms.Controllers
{
    [Route("~/api/holder/{action}"), ApiGroup("新闻管理")]
    public class PlaceholderApiController : ApiController
    {
        [Route("~/api/holder/img{width:int}x{height:int}"), HttpGet, Api("获取占位符")]
        public ActionResult ImageHolder(int width, int height)
        {
            string text = Get("text", string.Empty);
            SKBitmap bitmap = CreateImagePlaceHolder(width, height, text);
            return Image(bitmap, SKEncodedImageFormat.Jpeg);
        }
    }
}
