namespace Ricebird.Wechat.Controllers
{
    [Route("~/wechat/[action]")]
    public class WechatController : Controller
    {
        public ActionResult Test()
        {
            return View();
        }
    }
}
