using Ricebird.Wechat.Services;

namespace Ricebird.Wechat.Controllers
{
    [Route("~/debug/wechat/[action]")]
    public class WechatDebugController(CorpWechatService corpService) : DebugController
    {
        public ActionResult GetTickets()
        {
            return Ok(new
            {
                success = true,
                msg = "",
                accessToken = corpService.AccessToken,
                ticket = corpService.JsApiTicket
            });
        }

        public ActionResult GetWechatLogin()
        {
            string url = Get("url", $"{Client.HostWithScheme}/mobile");
            if (url.HasValue())
            {
                string final = corpService.GetAuthorizeLogin(Client, url);
                return Ok(new
                {
                    success = true,
                    msg = "",
                    url = final,
                    src = url
                });
            }
            return Ok();
        }
    }
}
