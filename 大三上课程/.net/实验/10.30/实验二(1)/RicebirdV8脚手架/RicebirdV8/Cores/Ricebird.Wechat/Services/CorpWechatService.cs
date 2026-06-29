using Ricebird.Framework.ShortUrl;
using Tencent;

namespace Ricebird.Wechat.Services
{
    public class CorpWechatService(IOptionService OptionService, IShortUrlService urlService) : ISingletonDependency
    {
        #region accessToken
        private readonly MemoryCache AccessTokenCache = new MemoryCache(new MemoryCacheOptions());
        internal CorpWechatOption Options { get; set; } = OptionService.LoadOptions<CorpWechatOption>();

        protected string GetAccessToken()
        {
            var opt = Options;
            string? value = AccessTokenCache.Get<string>("token");
            if (value != null)
            {
                return value;
            }

            var msg = RequestUtility.HttpGet($"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={opt.CorpId}&corpsecret={opt.CorpSecert}");
            var token = msg.GetJsonValue("access_token", "");
            AccessTokenCache.Set("token", opt =>
            {
                opt.AbsoluteExpiration = DateTime.Now.AddSeconds(7000);
                return token;
            });
            return token;
        }

        internal string GetJsApiTicket()
        {
            string? value = AccessTokenCache.Get<string>("jsticket");
            if (value != null)
            {
                return value;
            }

            var msg = RequestUtility.HttpGet($"https://qyapi.weixin.qq.com/cgi-bin/get_jsapi_ticket?access_token={AccessToken}");
            var token = msg.GetJsonValue("ticket", "");
            if (token.HasValue())
            {
                AccessTokenCache.Set("jsticket", opt =>
                {
                    opt.AbsoluteExpiration = DateTime.Now.AddSeconds(7000);
                    return token;
                });
            }
            return token;
        }

        public string AccessToken => GetAccessToken();
        public string JsApiTicket => GetJsApiTicket();
        #endregion

        #region 回调处理
        public string GetCallbackMessage(IClient client, string signature, int timestamp, string nonce, string echostr)
        {
            var opt = Options;
            string[] dict = [opt.Token, timestamp.ToString(), nonce, echostr];
            string str = string.Join(null, dict.OrderBy(e => e));
            string sig = SecureHelper.GetSha1(str);
            if (sig != signature)
            {
                return "SIGNATURE INCORRECT";
            }

            var (msg, _) = DecryptAes(client, echostr, opt);
            return msg;
        }
        #endregion

        private static (string msg, string recieveId) DecryptAes(IClient client, string echostr, CorpWechatOption opt)
        {
            try
            {
                //Aes aes = Aes.Create();
                //aes.KeySize = 256;
                //aes.BlockSize = 128;
                //byte[] aesKey = Convert.FromBase64String(opt.AesKey + "=");
                //byte[] aesMsg = Convert.FromBase64String(echostr);
                //byte[] iv = new byte[16];
                //Array.Copy(aesKey, iv, 16);
                //aes.Key = aesKey;
                //byte[] randMsg = aes.DecryptCbc(aesMsg, iv);
                //client.Log(MODULE_NAME, "DecryptAes", "", $"解码成功，总长度:{randMsg.Length}。");

                //int len = BitConverter.ToInt32(randMsg, 16);
                //client.Log(MODULE_NAME, "DecryptAes", "", $"解码完成，长度:{len}。");
                //byte[] msgBytes = new byte[len];
                //byte[] recieveIdBytes = new byte[randMsg.Length - 20 - len];
                //Array.Copy(randMsg, 20, msgBytes, 0, len); ;
                //Array.Copy(randMsg, 20 + len, recieveIdBytes, 0, randMsg.Length - 20 - len);
                //string msg = Encoding.UTF8.GetString(msgBytes);
                //string recieveId = Encoding.UTF8.GetString(recieveIdBytes);
                //client.Log(MODULE_NAME, "DecryptAes", "", $"解码结果:{msg}。回复ID号： {recieveId}");
                string recieveId = "";
                string msg = Cryptography.AES_decrypt(echostr, opt.AesKey, ref recieveId);
                return (msg, recieveId);
            }
            catch (Exception ex)
            {
                client.LogException(ex, MODULE_NAME, "DecryptAes");
                return ("DES FAILURE", "DES FAILURE");
            }
        }

        #region 获取授权链接
        public string GetAuthorizeUrl(IClient client, string returnUrl)
        {
            return GetAuthorizeUrl(client, returnUrl, string.Empty);
        }

        public string GetAuthorizeUrl(IClient client, string returnUrl, string state)
        {
            var opt = Options;
            string url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={opt.CorpId}&redirect_uri={returnUrl}&response_type=code&scope=snsapi_base&state={state}&agentid={opt.AgentId}#wechat_redirect";
            return url;
        }

        public string GetOpenIdByCode(IClient client)
        {
            string code = client.Get("code", string.Empty);
            string url = $"https://qyapi.weixin.qq.com/cgi-bin/auth/getuserinfo?access_token={AccessToken}&code={code}";
            string msg = RequestUtility.HttpGet(url);
            if (!msg.Contains("\"errmsg\":\"ok\""))
            {
                client.Log(MODULE_NAME, "GetOpenIdByCode", "", $"{msg}");
            }

            var userId = msg.GetJsonValue("userid", "");
            return userId;
        }
        #endregion

        #region 授权登录接口
        public string GetAuthorizeLogin(IClient client, string returnUrl, TimeSpan duration)
        {
            string state = urlService.ToShortUrlCode(client, returnUrl, duration);
            string url = GetAuthorizeUrl(client, $"{client.HostWithScheme}/api/wechat/corp/loginCallback", state);
            string shortUrl = urlService.ToShortUrl(client, url, duration);

            return shortUrl;
        }

        public string GetAuthorizeLogin(IClient client, string returnUrl)
        {
            string state = urlService.ToPermanentUrlCode(client, returnUrl);
            string url = GetAuthorizeUrl(client, $"{client.HostWithScheme}/api/wechat/corp/loginCallback", state);
            string shortUrl = urlService.ToPermanentUrl(client, url);

            return shortUrl;
        }
        #endregion

        #region 企业微信发送通知
        public void SendText(IClient client, string openId, string text)
        {
            var opt = Options;
            var jsonObj = new
            {
                touser = openId,
                msgtype = "text",
                agentid = opt.AgentId,
                text = new
                {
                    content = text,
                },
                enable_duplicate_check = 0,
                duplicate_check_interval = 7200,
            };

            SendMessage(client, jsonObj);
        }

        public void SendTextNotice(IClient client, string openId, string mainTitle, string mainDesc, List<(string keyname, string value)> horizontalContentList, List<(string title, string url)> jumpList, string cardAction)
        {
            var opt = Options;
            var jsonObj = new
            {
                touser = openId,
                msgtype = "template_card",
                agentid = opt.AgentId,
                template_card = new
                {
                    card_type = "text_notice",
                    main_title = new
                    {
                        title = mainTitle,
                        desc = mainDesc,
                    },
                    horizontal_content_list = horizontalContentList.Select(e => new
                    {
                        e.keyname,
                        e.value
                    }),
                    jumpList = jumpList.Select(e => new
                    {
                        type = 1,
                        e.title,
                        e.url
                    }),
                    card_action = new
                    {
                        type = 1,
                        url = cardAction
                    },
                    enable_duplicate_check = 0,
                    duplicate_check_interval = 7200,
                }
            };

            SendMessage(client, jsonObj);
        }

        public void SendMessage(IClient client, object json)
        {
            string url = $"https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={AccessToken}";
            string msg = RequestUtility.HttpPost(url, json);
            if (!msg.Contains("\"errmsg\":\"ok\""))
            {
                client.Log(MODULE_NAME, "SendMessage", json.SearializeJson(), $"微信返回: {msg}");
            }
        }
        #endregion

        #region 企业微信JSAPI注册
        /// <summary>
        /// 用在JS里的签名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public (string appId, string timestamp, string nonceStr, string signature) GetSignature(string url)
        {
            string timestamp = DateTime.Now.ToUnixMillis().ToString();
            string nonceStr = GenerateId(8);
            string srcStr = $"jsapi_ticket={JsApiTicket}&noncestr={nonceStr}&timestamp={timestamp}&url={url}";
            string sig = SecureHelper.GetSha1(srcStr);

            return (Options.CorpId, timestamp, nonceStr, sig);
        }
        #endregion
    }
}
