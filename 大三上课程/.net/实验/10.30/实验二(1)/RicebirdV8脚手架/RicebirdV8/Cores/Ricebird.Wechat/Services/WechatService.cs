namespace Ricebird.Wechat.Services
{
    public class WechatService(IOptionService OptionService) : ISingletonDependency
    {

        #region accessToken
        private readonly MemoryCache AccessTokenCache = new MemoryCache(new MemoryCacheOptions());
        private WechatOption Options { get; set; } = OptionService.LoadOptions<WechatOption>();

        protected string GetAccessToken()
        {
            string? value = AccessTokenCache.Get<string>("token");
            if (value != null)
            {
                return value;
            }

            var msg = RequestUtility.HttpGet($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={Options.AppId}&secret={Options.AppSecert}");
            var token = msg.GetJsonValue("access_token", "");
            AccessTokenCache.Set("token", opt =>
            {
                opt.AbsoluteExpiration = DateTime.Now.AddSeconds(7000);
                return token;
            });
            return token;
        }

        public string AccessToken => GetAccessToken();
        #endregion

        public bool MsgCheck(string msg, IClient client)
        {
            if (string.IsNullOrWhiteSpace(client.CurrentUser.OpenId))
            {
                return true;
            }

            string url = $"https://api.weixin.qq.com/wxa/msg_sec_check?access_token={AccessToken}";
            object para = new
            {
                content = msg,
                version = 2,
                scene = 3,
                openid = client.CurrentUser.OpenId
            };
            string repon = RequestUtility.HttpPost(url, para);

            // Logger.Log(client, "WechatService", "MsgCheck", "", $"微信返回：{repon}，调用参数：{para.SearializeJson()}，调用地址：{url}");
            string[] notAllows = ["10001", "20001", "20002", "20003", "20006", "20008", "20012", "20013", "21000"];
            foreach (var item in notAllows)
            {
                if (repon.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
