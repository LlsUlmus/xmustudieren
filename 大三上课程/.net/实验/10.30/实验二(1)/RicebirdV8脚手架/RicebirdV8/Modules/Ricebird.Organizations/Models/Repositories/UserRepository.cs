using Ricebird.Security.Models;

namespace Ricebird.Organizations.Models
{
    public class UserRepository(RicebirdContext ctx, IServiceProvider scoped, IOrgService orgService) : RepositoryBase<User>(ctx, scoped), IUserRepository, IUserStore
    {
        public DbSet<UserRelationship> UserRelationships => DbContext.Set<UserRelationship>();
        public DbSet<RoleSchema> RoleSchemas => DbContext.Set<RoleSchema>();
        public DbSet<Department> Departments => DbContext.Set<Department>();

        #region 获取用户
        public User? GetUser(Guid id)
        {
            var user = DbSet.FirstOrDefault(e => e.ID == id);
            return user;
        }

        public User? GetUserByCode(string code)
        {
            var user = DbSet.FirstOrDefault(e => e.Code == code);
            return user;
        }

        public List<User> GetUsers(IEnumerable<Guid> ids)
        {
            var users = DbSet.Where(e => ids.Contains(e.ID)).ToList();
            return users;
        }

        public IQueryable<User> GetUserByCodeAndRole(string code, Guid roleId) => DbSet.Where(e => e.Code == code && UserRelationships.Any(x => x.RoleId == roleId && x.UserId == e.ID));

        public IQueryable<User> GetExpertsByCode(Guid roleId, Guid departId)
        {
            if (departId == Guid.Empty)
            {
                return DbSet.Where(e => UserRelationships.Any(x => x.RoleId == roleId && x.UserId == e.ID));
            }
            else
            {
                return DbSet.Where(e => UserRelationships.Any(x => x.RoleId == roleId && x.DepartId == departId && x.UserId == e.ID));
            }
        }

        public User? GetUser(string token)
        {
            var user = DbSet.AsNoTracking().FirstOrDefault(e => e.UserName == token
                || (e.Code == token && e.Code != null && e.Code != "")
                || (e.Email == token && e.Email != null && e.Email != "")
                || (e.Mobile == token && e.Mobile != null && e.Mobile != ""));
            return user;
        }

        CommonUser IUserStore.GetUser(string token)
        {
            User? user = GetUser(token);
            if (user == null)
            {
                return new(Guid.Empty, "找不到对应用户", string.Empty, string.Empty, string.Empty, string.Empty, UserStatus.Disable, string.Empty, AccessLevel.AllDeny, string.Empty, Guid.Empty);
            }

            return new CommonUser(user.ID, user.RealName, user.Avatar, user.Code, user.Mobile, user.Email, user.AuditStatus, user.UserPassword, user.Level, user.OpenId, user.RootDepartId);
        }

        CommonUser IUserStore.GetUserByCode(string code)
        {
            User? user = GetUserByCode(code);
            if (user == null)
            {
                return new(Guid.Empty, "找不到对应用户", string.Empty, string.Empty, string.Empty, string.Empty, UserStatus.Disable, string.Empty, AccessLevel.AllDeny, string.Empty, Guid.Empty);
            }

            return new CommonUser(user.ID, user.RealName, user.Avatar, user.Code, user.Mobile, user.Email, user.AuditStatus, user.UserPassword, user.Level, user.OpenId, user.RootDepartId);
        }

        public List<(Guid roleId, Guid departId, string departName)> GetRelations(Guid userId)
        {
            var query = GetRelationships(userId);

            var data = query.Select(e =>
            {
                string depart = orgService.GetDepartById(e.DepartId)?.Name ?? "";
                return (e.RoleId, e.DepartId, depart);
            })
            .ToList();

            return data;
        }

        public List<UserRelationship> GetRelationships(Guid userId)
        {
            var data = (from ur in UserRelationships
                        join r in RoleSchemas on ur.RoleId equals r.ID
                        where ur.UserId == userId
                        orderby r.DisplayOrder
                        select ur).ToList();

            return data;
        }

        public (bool success, List<string> errors, User user) CreateAdministrator(string userName, string code, string mobile, string email, string realName, string pwd)
        {
            User? u = FirstOrDefault(e => e.Code == code);
            if (u != null)
            {
                return (true, new List<string>(), u);
            }

            u = CreateNew();
            u.UserPassword = pwd;
            u.Level = AccessLevel.AllAccess;
            u.UserName = userName;
            u.Mobile = mobile;
            u.Email = email;
            u.Code = code;
            u.RealName = realName;
            u.UserSource = "系统生成";

            var result = u.Validate(Client);

            if (!result)
            {
                return (false, result.ErrorStrings, u);
            }

            DbSet.Add(u);
            SaveChanges();

            return (true, new List<string>(), u);
        }
        #endregion

        #region 保存用户
        //public DbOperate SaveUser(User entity, string userSource)
        //{
        //    string[] ignoreProperties = ["UserPassword", "LockTo", "UserSource"];

        //    Guid id = entity.ID;
        //    DbOperate ret = DbOperate.Update;
        //    if (id != Guid.Empty)
        //    {
        //        User? exists = DbSet.FirstOrDefault(e => e.ID == id);
        //        if (exists == null)
        //        {
        //            exists = new User();
        //            DbSet.Add(exists);
        //            exists.UserSource = userSource;
        //            exists.CopyFromObject(entity);
        //            ret = DbOperate.Create;
        //        }
        //        else
        //        {
        //            exists.CopyFromObject(entity, ignoreProperties);
        //        }

        //        SaveChanges();

        //        return ret;
        //    }

        //    return DbOperate.None;
        //}
        #endregion

        #region OpenId相关
        //public IUserIdentity? ReleaseOpenId(string openId)
        //{
        //    var user = DbSet.FirstOrDefault(e => e.OpenId == openId);
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    user.OpenId = string.Empty;
        //    SaveChanges();
        //    return user;
        //}

        //public IUserIdentity? ReleaseOpenId(Guid userId)
        //{
        //    var user = DbSet.FirstOrDefault(e => e.ID == userId);
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    user.OpenId = string.Empty;
        //    SaveChanges();
        //    return user;
        //}

        //public IUserIdentity? BindOpenId(Guid userId, string openId)
        //{
        //    var user = DbSet.FirstOrDefault(e => e.ID == userId);
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    user.OpenId = openId;
        //    SaveChanges();
        //    return user;
        //}

        //public IUserIdentity? GetOrCreateUserByOpenId(string openId)
        //{
        //    var user = DbSet.AsNoTracking().FirstOrDefault(e => e.OpenId == openId);

        //    if (user == null)
        //    {
        //        string userKey = GenerateKey(6);
        //        user = CreateNew();
        //        user.UserPassword = InitializePasssword;
        //        user.UserName = $"新用户_{userKey}";
        //        user.Mobile = "";
        //        user.Email = "";
        //        user.Code = userKey;
        //        user.RealName = $"新用户_{userKey}";
        //        user.OpenId = openId;
        //        user.UserType = "wechat";
        //        user.AuditStatus = UserStatus.SystemAutoRegistion;
        //        Save(user);
        //    }

        //    return user;
        //}
        #endregion

        #region 用户验证
        /// <summary>
        /// 验证用户名是否可以使用
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool ExistUserName(string userName, Guid userId)
        {
            return !string.IsNullOrWhiteSpace(userName) && DbSet.Any(e => e.UserName == userName && e.ID != userId);
        }

        /// <summary>
        /// 验证手机是否可以使用
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool ExistMobile(string mobile, Guid userId)
        {
            return !string.IsNullOrWhiteSpace(mobile) && DbSet.Any(e => e.Mobile == mobile && e.ID != userId);
        }

        /// <summary>
        /// 验证邮箱是否可以使用
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool ExistEmail(string email, Guid userId)
        {
            return !string.IsNullOrWhiteSpace(email) && DbSet.Any(e => e.Email == email && e.ID != userId);
        }

        /// <summary>
        /// 验证学工号是否可以使用
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool ExistCode(string code, Guid userId)
        {
            return !string.IsNullOrWhiteSpace(code) && DbSet.Any(e => e.Code == code && e.ID != userId);
        }
        #endregion

        #region 用户完整性验证
        //public (bool success, string message, User? user, List<string> completions) GetUserCompletion(Guid userId)
        //{
        //    User? user = DbSet.FirstOrDefault(e => e.ID == userId);
        //    if (user == null)
        //    {
        //        return (false, "找不到用户", null, new List<string>());
        //    }

        //    List<string> completions = user.CheckCompletion().ToList();
        //    return (true, "", user, completions);
        //}
        #endregion
    }
}
