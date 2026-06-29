using Ricebird.Framework.Clients;

namespace Ricebird.Framework.Diagnostics
{
    public interface IDbLogger
    {
        void LogException(IClient Client, Exception ex, string module, string method);
        void Log(IClient Client, string module, string method, string relateId, string desc);

        IUserLogger User { get; }
    }
}
