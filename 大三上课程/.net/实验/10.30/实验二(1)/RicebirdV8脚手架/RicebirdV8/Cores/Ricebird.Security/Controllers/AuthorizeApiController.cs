using AngleSharp.Dom;
using Ricebird.Framework.ShortUrl;
using Ricebird.Security.ViewModels;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ricebird.Security.Controllers
{
    [Route("~/api/authorize/[action]"), ApiGroup("登录与授权")]
    public partial class AuthorizeApiController(SecurityService securityService, HostEnv env, IDataDictionaryService dictService) : ApiController
    {
        [Api("登录")]
        public ActionResult Login()
        {
            string loginToken = Get("token", string.Empty);
            string nounce = Get(nameof(nounce), string.Empty);
            long timestamp = Get(nameof(timestamp), 0L);
            string signature = Get(nameof(signature), string.Empty);

            if (string.IsNullOrWhiteSpace(nounce))
            {
                return Fail("必须输入随机字符串");
            }

            DateTime time = FromUnixMillis(timestamp);
            if (DateTime.Now - time > TimeSpan.FromSeconds(10))
            {
                return Fail($"不正确的时间戳，现在的服务器时间是{DateTime.Now:yyyy/M/d H:m:s}");
            }

            if (!sha256().Match(signature).Success)
            {
                return Fail("无效的签名");
            }

            (bool success, string msg, string token, IUserPrincipal data) = securityService.GetCredential(loginToken, signature, Client, pwd =>
            {
                // 签名生成算法：
                string[] @params = [Client.Method.ToUpper(), Client.ApiPath, timestamp.ToString(), nounce, loginToken, pwd];
                string str = @params.JoinAsString('\n');
                var result = SecureHelper.GetSha256(str);
                return result;
            });

            if (success)
            {
                Client.Features.Set(new AccessToken(ConstKeys.AuthenticationKey, token));
                Client.Logger.User.SignInUser(Client, data);
                Response.Cookies.Append(ConstKeys.AuthenticationKey, token);
            }

            return Ok(new
            {
                success,
                msg,
                token,
                data
            });
        }

        [Api("注销")]
        public ActionResult Logout()
        {
            string loginToken = Client.GetInRequest(ConstKeys.AuthenticationKey, string.Empty);
            securityService.Logout(loginToken);
            string srcUrl = Get(nameof(srcUrl), string.Empty);
            if (!srcUrl.IsNullOrEmpty() && srcUrl.StartsWith(Client.HostWithScheme))
            {
                return Redirect(srcUrl);
            }
            else
            {
                return Ok();
            }
        }

        [ApiShouldAuthorize("踢除用户")]
        public ActionResult BanUser()
        {
            string code = Client.Get("code", string.Empty);

            securityService.RemoveUser(code);

            return Ok();
        }

        [Api("验证用户身份")]
        public ActionResult ValidateToken()
        {
            string loginToken = Client.GetInRequest(ConstKeys.AuthenticationKey, string.Empty);
            string msg = "";
            string token = loginToken;

            if (loginToken.HasValue())
            {
                var user = securityService.GetUserPrinciple(loginToken);
                if (user != null)
                {
                    return Ok(new
                    {
                        success = true,
                        msg,
                        token,
                        data = user,
                        platform = Browser.Platform,
                        system = Browser.System,
                        isMobile = Browser.IsMobile,
                        isWechat = Browser.IsWechat && !Browser.IsWorkWechat,
                        isWorkWechat = Browser.IsWorkWechat,
                    });
                }
            }

            return Fail(new
            {
                success = false,
                msg = "无效的token",
                token = "",
                data = securityService.Anonymous,
                platform = Browser.Platform,
                system = Browser.System,
                isMobile = Browser.IsMobile,
                isWechat = Browser.IsWechat && !Browser.IsWorkWechat,
                isWorkWechat = Browser.IsWorkWechat,
            });
        }

        [GeneratedRegex("[0-9a-z]{64}")]
        private static partial Regex sha256();
    }
}
