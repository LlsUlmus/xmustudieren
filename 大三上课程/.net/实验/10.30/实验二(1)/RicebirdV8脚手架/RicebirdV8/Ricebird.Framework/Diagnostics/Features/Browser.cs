using System.Text.RegularExpressions;

namespace Ricebird.Framework.Diagnostics.Features
{
    public partial class Browser
    {
        public string UserAgent;

        public Browser(HttpContext ctx)
            : this(ctx.Request.Headers.UserAgent.ToString() ?? "")
        {

        }

        public Browser(string userAgent)
        {
            UserAgent = userAgent;
            Recognize();
            RecognizeSystem();
        }

        private readonly Regex frameworkRegex = framework();
        private readonly Regex msieRegex = msie();
        private readonly Regex tridentRegex = trident();
        private readonly Regex chromeRegex = chrome();
        private readonly Regex safariRegex = safari();
        private readonly Regex operaRegex = opera();
        private readonly Regex edgeRegex = edge();
        private readonly Regex wechatRegex = wechat();
        private readonly Regex postmanRegex = postman();
        private readonly Regex apifoxRegex = apifox();
        private readonly Regex firefoxReges = firefox();

        public void Recognize()
        {
            bool matchAny = false;

            #region 系统内
            var match = frameworkRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = match.Groups["serviceName"].Value;
                return;
            }
            #endregion

            #region POSTMAN
            match = postmanRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = "Postman";
                return;
            }
            #endregion

            #region Apifox
            match = apifoxRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = "ApiFox";
                return;
            }
            #endregion

            #region IE系列
            match = msieRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"IE";
                matchAny = true;
            }

            match = tridentRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"IE兼容模式";
                matchAny = true;
            }

            match = edgeRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"Edge";
                return;
            }
            #endregion

            #region Chrome系列
            match = chromeRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"Chrome";
                matchAny = true;
            }
            #endregion

            #region Safari系列
            match = safariRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"Safari";
                matchAny = true;
            }
            #endregion

            #region Opera系列
            match = operaRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"Opera";
                matchAny = true;
            }
            #endregion

            #region 火狐系列
            match = firefoxReges.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                Platform = $"Firefox";
                return;
            }
            #endregion

            #region 国产浏览器
            if (UserAgent.Contains("360SE"))
            {
                Platform = "360浏览器";
                matchAny = true;
            }

            if (UserAgent.Contains("QQBrowser"))
            {
                Platform = "QQ浏览器";
                matchAny = true;
            }

            match = wechatRegex.Match(UserAgent);
            if (match.Success)
            {
                MajorVersion = Convert.ToInt32(match.Groups["version"].Value);
                IsWechat = true;
                IsMobile = !UserAgent.Contains("WindowsWechat");
                IsWorkWechat = UserAgent.Contains("wxwork"); // 包含这个的就是企业微信
                Platform = IsWorkWechat ? "企业微信" : "微信";
                matchAny = true;
            }

            if (!matchAny)
            {
                Platform = $"其它({UserAgent})";
            }
            #endregion
        }

        public void RecognizeSystem()
        {
            bool matchAny = false;
            if (UserAgent.Contains("Windows"))
            {
                System = "Windows";
                return;
            }

            if (UserAgent.Contains("Linux"))
            {
                System = "Linux";
                matchAny = true;
            }

            if (UserAgent.Contains("Android"))
            {
                System = "Android";
                IsMobile = true;
                matchAny = true;
            }

            if (UserAgent.Contains("iPhone"))
            {
                System = "iPhone";
                IsMobile = true;
                matchAny = true;
            }

            if (UserAgent.Contains("RicebirdFramework"))
            {
                System = "内部";
                return;
            }

            if (UserAgent.Contains("Apifox"))
            {
                System = "调试端";
                return;
            }

            if (UserAgent.Contains("Postman"))
            {
                System = "调试端";
                return;
            }

            if (!matchAny)
            {
                System = "其它";
            }
        }

        public string Platform { get; set; } = string.Empty;
        public int MajorVersion { get; set; } = 0;
        public string System { get; set; } = "PC";
        public bool IsMobile { get; set; } = false;
        /// <summary>
        /// 是否是微信端
        /// </summary>
        public bool IsWechat { get; set; } = false;
        /// <summary>
        /// 是否是企业微信端
        /// </summary>
        public bool IsWorkWechat { get; set; } = false;

        public override string ToString() => IsWechat ? Platform : $"{System}端{Platform} {MajorVersion}";
        [GeneratedRegex(@"RicebirdFramework\/(?<version>\d+) (?<serviceName>\w+)")]
        private static partial Regex framework();
        [GeneratedRegex(@"MSIE (?<version>\d+)")]
        private static partial Regex msie();
        [GeneratedRegex(@"Trident.*rv:(?<version>\d+)")]
        private static partial Regex trident();
        [GeneratedRegex(@"Chrome\/(?<version>\d+)")]
        private static partial Regex chrome();
        [GeneratedRegex(@"Version\/(?<version>\d+).*Safari")]
        private static partial Regex safari();
        [GeneratedRegex(@"Opera.*Version\/(?<version>\d+)")]
        private static partial Regex opera();
        [GeneratedRegex(@"Edg\/(?<version>\d+)")]
        private static partial Regex edge();
        [GeneratedRegex(@"Firefox\/(?<version>\d+)")]
        private static partial Regex firefox();
        [GeneratedRegex(@"MicroMessenger\/(?<version>\d+)")]
        private static partial Regex wechat();
        [GeneratedRegex(@"Postman.*/(?<version>\d+)")]
        private static partial Regex postman();
        [GeneratedRegex(@"Apifox.*/(?<version>\d+)")]
        private static partial Regex apifox();
    }
}
