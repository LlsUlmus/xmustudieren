using Ricebird.Framework.Clients;
using Ricebird.Framework.Organizations;
using Ricebird.Framework.Security;

namespace Ricebird.Framework.Diagnostics
{
    public interface IUserLogger
    {
        void CreateUser(IClient client, IUser user);
        void EditUser(IClient client, IUser user);
        void RegisterUser(IClient client, IUser user);
        void RemoveUser(IClient client, IUser user);
        void SignInUser(IClient client, IUserPrincipal user);
    }
}