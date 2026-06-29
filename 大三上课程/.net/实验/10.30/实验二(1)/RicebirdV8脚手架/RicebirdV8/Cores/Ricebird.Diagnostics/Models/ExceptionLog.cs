using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.Clients;

namespace Ricebird.Diagnostics.Models
{
    [Index(nameof(CreatedOn), AllDescending = true)]
    public class ExceptionLog
    {
        #region ctor
        public ExceptionLog() { }
        public ExceptionLog(IClient client, Exception ex, string module, string method)
        {
            IpAddress = client.RealIp;
            ModuleName = string.IsNullOrEmpty(module) ? client.ApiPath : module;
            MethodName = string.IsNullOrEmpty(method) ? client.Method : method;
            Message = ex.Message;
            StackTrace = ex.StackTrace ?? "";
            if (ex.InnerException != null)
            {
                StackTrace += $"\n------\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }
            QueryString = client.Params; // (client.HttpContext?.Request.Query.Select(e => $"{e.Key}={e.Value}").JoinAsString('&')) ?? "";
            RequestPayload = client.PostStream;
            CreatedBy = client.CurrentUser.RealName;
        }
        #endregion

        #region 数据库字段
        public Guid ID
        {
            get; set;
        } = SequentialGuid.NewSuid();

        public string IpAddress
        {
            get; set;
        } = string.Empty;

        public string ModuleName
        {
            get; set;
        } = string.Empty;

        public string MethodName
        {
            get; set;
        } = string.Empty;

        public string Message
        {
            get; set;
        } = string.Empty;

        public string StackTrace
        {
            get; set;
        } = string.Empty;

        public string QueryString
        {
            get; set;
        } = string.Empty;

        public string RequestPayload
        {
            get; set;
        } = string.Empty;

        public string CreatedBy
        {
            get; set;
        } = string.Empty;

        public DateTime CreatedOn
        {
            get; set;
        } = DateTime.Now;
        #endregion
    }
}
