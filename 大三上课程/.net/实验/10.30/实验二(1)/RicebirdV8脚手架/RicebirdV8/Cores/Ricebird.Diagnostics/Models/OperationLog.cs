using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.Clients;

namespace Ricebird.Diagnostics.Models
{
    [Index(nameof(CreatedOn), AllDescending = true)]
    public class OperationLog
    {
        #region ctor
        public OperationLog() { }
        public OperationLog(IClient client, string module, string method, string relateId, string desc)
        {
            IpAddress = client.RealIp;
            ModuleName = module;
            MethodName = method;
            RelateId = relateId;
            Description = desc;
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

        public string RelateId
        {
            get; set;
        } = string.Empty;

        public string Description
        {
            get;
            set;
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
