namespace Ricebird.ShortUrls.Controllers
{
    public class ShortController : RicebirdController
    {
        [Route("~/404")]
        public ActionResult PageNotFound()
        {
            return ErrorPage("找不到页面");
        }
    }
}
