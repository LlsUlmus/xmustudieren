using Ricebird.Framework.Clients;
using Ricebird.Framework.Organizations;
using Ricebird.Framework.Security;

namespace Ricebird.Diagnostics.Services
{
    public class UserLogger(DiagnosticsContext dctx) : IUserLogger
    {
        private DiagnosticsContext DbContext { get; set; } = dctx;

        public void Log(string oper, string desc, IUser user, IClient client)
        {
            UserLog log = new UserLog()
            {
                Operation = oper,
                Decription = desc,
                RealIp = client.RealIp,
                RealName = user.RealName,
                UserID = user.ID,
                UserAgent = client.UserAgent,
                CreatedBy = user.RealName,
                CreatedOn = DateTime.Now
            };

            DbContext.Add(log);
            DbContext.SaveChanges();
        }

        public void Log(string oper, string desc, IUserPrincipal user, IClient client)
        {
            UserLog log = new UserLog()
            {
                Operation = oper,
                Decription = desc,
                RealIp = client.RealIp,
                RealName = user.RealName,
                UserID = user.ID,
                UserAgent = client.UserAgent ?? "",
                CreatedBy = user.RealName,
                CreatedOn = DateTime.Now
            };

            DbContext.Add(log);
            DbContext.SaveChanges();
        }

        public void CreateUser(IClient client, IUser user) => Log("创建", $"由{client.CurrentUser}新建了用户{user}", user, client);

        public void RegisterUser(IClient client, IUser user) => Log("注册", $"用户{user}于{DateTime.Now:yyyy年M月d日 H时m分}注册", user, client);

        public void SignInUser(IClient client, IUserPrincipal user) => Log("登录", $"用户通过{client.Browser}登录", user, client);

        public void EditUser(IClient client, IUser user) => Log("修改", $"由{client.CurrentUser}修改了用户", user, client);

        public void RemoveUser(IClient client, IUser user) => Log("删除", $"由{client.CurrentUser}删除了用户", user, client);
    }
}
