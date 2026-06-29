using Ricebird.Framework.ShortUrl;
using Ricebird.Security.Services;
using Ricebird.Wechat.Services;
using Ricebird.Wechat.ViewModels;

namespace Ricebird.Wechat.Controllers
{
    /// <summary>
    /// 企业微信相关方法
    /// </summary>
    /// <param name="wechatService"></param>
    /// <param name="urlService"></param>
    /// <param name="opt"></param>
    [Route("~/api/wechat/corp/[action]")]
    public class CorpWechatApiController(ISmsService sender, CorpWechatService wechatService, IShortUrlService urlService, SecurityService securityService) : ApiController
    {
        #region 处理Http回调
        public ActionResult GetCallback()
        {
            string signature = Get("msg_signature", string.Empty);
            int timestamp = Get("timestamp", 0);
            string nonce = Get("nonce", string.Empty);
            string echostr = Get("echostr", string.Empty);

            string msg = wechatService.GetCallbackMessage(Client, signature, timestamp, nonce, echostr);

            return Content(msg);
        }
        #endregion

        #region 发送URL获取OpenId
        public ActionResult GetCorpAuthorizeUrl()
        {
            string connId = Get("connId", string.Empty);

            // 获取企业微信的授权链接
            string returnUrl = $"{Client.HostWithScheme}/api/wechat/corp/callback?connId={connId}".UrlEncode();
            string url = wechatService.GetAuthorizeUrl(Client, returnUrl, "1");
            string shortUrl = urlService.ToShortUrl(Client, url);
            // sender.SendToClient(connId, "corp-callback-success", true);
            return Ok(new
            {
                success = true,
                msg = "",
                url = shortUrl,
            });
        }

        public ActionResult Callback()
        {
            string connId = Get("connId", string.Empty);
            var userId = wechatService.GetOpenIdByCode(Client);
            bool success = userId.HasValue();
            sender.SendToClient(connId, "corp-callback-complete", success, userId);
            return success ? SuccessPage("绑定成功") : ErrorPage("绑定失败<br/>请确认您已经加入企业微信");
        }
        #endregion

        #region 获取地理信息
        public ActionResult GetGeoInfo()
        {
            string connId = Get("connId", string.Empty);
            string returnUrl = $"{Client.HostWithScheme}/api/wechat/corp/GeoCallback?connId={connId}".UrlEncode();
            string url = wechatService.GetAuthorizeUrl(Client, returnUrl, "1");
            string shortUrl = urlService.ToShortUrl(Client, url);

            return Ok(new
            {
                success = true,
                msg = "",
                url = shortUrl,
            });
        }

        public ActionResult GeoCallback()
        {
            string connId = Get("connId", string.Empty);
            sender.SendToClient(connId, "corp-callback-success", true);
            return View();
        }

        [HttpPost]
        public ActionResult PostGeoInfo()
        {
            string connId = Get("connId", string.Empty);
            string geo = Get(nameof(geo), string.Empty); // 格式是 `${latitude},${longitude}`
            sender.SendToClient(connId, "corp-callback-complete", true, geo);
            return Ok();
        }
        #endregion

        #region 企业微信登录回调
        public ActionResult LoginCallback()
        {
            string state = Get("state", string.Empty);
            string toUrl = urlService.ToLongUrl(state);
            if (string.IsNullOrWhiteSpace(toUrl))
            {
                return ErrorPage("该链接可能已经过期");
            }

            string userCode = wechatService.GetOpenIdByCode(Client);
            var (success, msg, token, user) = securityService.GetCredential(userCode, Client);

            if (success)
            {
                Client.Logger.User.SignInUser(Client, user);
                Response.Cookies.Append(ConstKeys.AuthenticationKey, token);
                WechatLoginCallbackViewModel vm = new WechatLoginCallbackViewModel(Client, user, token, toUrl);
                return View(vm);
            }
            else
            {
                return ErrorPage(msg);
            }
        }
        #endregion
    }
}
