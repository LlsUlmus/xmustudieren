using Ricebird.Framework.Clients;

namespace Ricebird.Diagnostics.Services
{
    public class DefaultLogger(DiagnosticsContext dctx) : IDbLogger
    {
        private DiagnosticsContext DbContext { get; set; } = dctx;

        public void LogException(IClient Client, Exception ex, string module, string method)
        {
            ExceptionLog exLog = new ExceptionLog(Client, ex, module, method);
            DbContext.ExceptionLogs.Add(exLog);
            DbContext.SaveChanges();
        }

        public void Log(IClient Client, string module, string method, string relateId, string desc)
        {
            OperationLog operationLog = new OperationLog(Client, module, method, relateId, desc);
            DbContext.OperationLogs.Add(operationLog);
            DbContext.SaveChanges();
        }

        public IUserLogger User { get; set; } = new UserLogger(dctx);
    }
}
