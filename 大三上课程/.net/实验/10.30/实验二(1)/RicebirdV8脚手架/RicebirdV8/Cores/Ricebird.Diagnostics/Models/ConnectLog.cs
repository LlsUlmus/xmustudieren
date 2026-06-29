using Ricebird.Framework.Diagnostics.Features;
using System.ComponentModel.DataAnnotations;

namespace Ricebird.Diagnostics.Models
{
    public class ConnectLog
    {
        #region 数据库字段
        public Guid ID { get; set; } = SequentialGuid.NewSuid();
        [MaxLength(15)]
        public string Ip { get; set; } = string.Empty;
        [MaxLength(8)]
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
        public string UserAgent { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int MajorVersion { get; set; } = 0;
        public string System { get; set; } = string.Empty;
        public bool IsMobile { get; set; } = false;
        public bool IsWechat { get; set; } = false;
        public int ResponseStatusCode { get; set; } = 0;
        public DateTime EndOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        #endregion

        #region ctor
        public ConnectLog() { }

        public ConnectLog(Browser browser, string ip, string method, string api)
        {
            Ip = ip;
            Method = method;
            Url = api;
            Duration = 0;
            UserAgent = browser.UserAgent;
            Platform = browser.Platform;
            MajorVersion = browser.MajorVersion;
            System = browser.System;
            IsMobile = browser.IsMobile;
            IsWechat = browser.IsWechat;
        }
        #endregion

        public void End(int statusCode)
        {
            ResponseStatusCode = statusCode;
            EndOn = DateTime.Now;
            Duration = (int)((EndOn - CreatedOn).TotalMilliseconds);
        }
    }
}
