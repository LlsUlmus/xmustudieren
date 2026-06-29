using Ricebird.Framework.Clients;
using Ricebird.Framework.Organizations;
using Ricebird.Framework.Security;

namespace Ricebird.Diagnostics.Services
{
    public class EmptyDbLogger : IDbLogger
    {
        public IUserLogger User
        {
            get; init;
        } = new EmptyUserLogger();

        public void Log(IClient Client, string module, string method, string relateId, string desc)
        {

        }

        public void LogException(IClient Client, Exception ex, string module, string method)
        {

        }
    }

    public class EmptyUserLogger : IUserLogger
    {
        public void CreateUser(IClient client, IUser user)
        {
        }
        public void EditUser(IClient client, IUser user) { }
        public void RegisterUser(IClient client, IUser user) { }
        public void RemoveUser(IClient client, IUser user) { }
        public void SignInUser(IClient client, IUserPrincipal user) { }
    }
}
