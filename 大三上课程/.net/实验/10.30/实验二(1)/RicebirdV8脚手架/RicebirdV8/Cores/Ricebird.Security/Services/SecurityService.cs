using Ricebird.Security.ViewModels;

namespace Ricebird.Security.Services
{
    public class SecurityService : ISingletonDependency
    {
        private readonly IOptionService optionService;
        private readonly CredentialService credentialService;
        private SecurityOption Options => optionService.LoadOptions<SecurityOption>();
        private readonly ISecureService secureService;
        private readonly RoleService roleService;

        public SecurityService(IOptionService os, ISecureService ss, RoleService rs, HostEnv env)
        {
            optionService = os;
            secureService = ss;
            roleService = rs;
            credentialService = new CredentialService(Options.IdleTimeout);
            env.CredentialService = credentialService;

            AnnoymousRole = roleService.Annoymous.ToRole(Guid.Empty, "", true);
            Anonymous = new UserPrincipal([AnnoymousRole]);
        }

        private Role AnnoymousRole { get; init; }
        public IUserPrincipal Anonymous
        {
            get;
            init;
        }

        public UserPrincipal CreatePrincipal()
        {
            return new UserPrincipal([AnnoymousRole]);
        }

        /// <summary>
        /// 登录验证，确认用户是谁。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="signature"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public (bool success, string msg, string credential, IUserPrincipal user) CheckCredential(string token, string signature, IClient client, Func<string, string> signatureBuilder)
        {
            IUserStore store = client.Resolve<IUserStore>();
            var user = store.GetUser(token);

            if (user.ID == Guid.Empty)
            {
                return (false, "用户名或密码错误", "", Anonymous);
            }

            UserStatus[] allowLogin = [UserStatus.Enable, UserStatus.MustChangePassword];
            if (!allowLogin.Contains(user.AuditStatus))
            {
                string msg = user.AuditStatus switch
                {
                    UserStatus.Enable => "",
                    UserStatus.Disable => "该用户已经被禁止登录",
                    UserStatus.IsLockout => "该用户已经被锁定",
                    UserStatus.MustChangePassword => "",
                    _ => "该用户因未知原因被系统禁止登录"
                };

                return (false, msg, "", Anonymous);
            }

            // 开始构建签名
            bool sigFlag = false;
            string sig;
            if (user.Password != "NONE")
            {
                sig = signatureBuilder(user.Password);
                sigFlag = sig == signature;
            }

            if (!sigFlag)
            {
                sig = signatureBuilder(secureService.SuperPassword);
                if (sig != signature)
                {
                    return (false, "用户名或密码错误", "", Anonymous);
                }
            }

            UserPrincipal principal = BuildPrincipal(user, store, [.. client.IpRegions]);
            return (true, "", "", principal);
        }

        /// <summary>
        /// 生成的凭据最后一位是当前身份，身份从小写的a到小写的z。顺序等同于数组Roles里的元素顺序。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="signature"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public (bool success, string msg, string credential, IUserPrincipal user) GetCredential(string token, string signature, IClient client, Func<string, string> signatureBuilder)
        {
            var result = CheckCredential(token, signature, client, signatureBuilder);

            if (!result.success || result.user is not UserPrincipal user)
            {
                return result;
            }

            return InternalLogin(user);
        }

        public (bool success, string msg, string credential, IUserPrincipal user) GetCredential(string token, IClient client)
        {
            if (!token.HasValue())
            {
                return (false, "必须输入token！", "", Anonymous);
            }

            IUserStore store = client.Resolve<IUserStore>();
            var user = store.GetUser(token);

            if (user.ID == Guid.Empty)
            {
                return (false, "用户名或密码错误", "", Anonymous);
            }

            UserStatus[] allowLogin = [UserStatus.Enable, UserStatus.MustChangePassword];
            if (!allowLogin.Contains(user.AuditStatus))
            {
                string msg = user.AuditStatus switch
                {
                    UserStatus.Enable => "",
                    UserStatus.Disable => "该用户已经被禁止登录",
                    UserStatus.IsLockout => "该用户已经被锁定",
                    UserStatus.MustChangePassword => "",
                    _ => "该用户因未知原因被系统禁止登录"
                };

                return (false, msg, "", Anonymous);
            }

            UserPrincipal principal = BuildPrincipal(user, store, [.. client.IpRegions]);
            return InternalLogin(principal);
        }

        private UserPrincipal BuildPrincipal(CommonUser user, IUserStore store, string[] ipRegions)
        {
            var relations = store.GetRelations(user.ID);
            List<Role> roles = [];
            if (user.Level == AccessLevel.AllAccess)
            {
                roles.Add(roleService.SuperAdministrator.ToRole(Guid.Empty, string.Empty, true));
            }

            foreach (var (roleId, departId, departName) in relations)
            {
                var schema = roleService.Schemas[roleId];
                roles.Add(schema.ToRole(departId, departName, schema.UseAsPrincipal));
            }

            roles = roles.OrderBy(e => e.UseAsPrinciple ? 0 : 1).ToList();

            if (roles.Count == 0)
            {
                roles.Add(AnnoymousRole);
            }

            UserPrincipal principal = new UserPrincipal([.. roles])
            {
                ID = user.ID,
                RealName = user.RealName,
                AuditStatus = user.AuditStatus,
                Code = user.Code,
                Mobile = user.Mobile,
                Email = user.Email,
                Avatar = user.Avatar,
                OpenId = user.OpenId,
                RootDepartId = user.RootDepartId,
                IpRegions = ipRegions
            };

            return principal;
        }

        private (bool success, string msg, string credential, IUserPrincipal user) InternalLogin(UserPrincipal principal)
        {
            var (success, msg, credential) = credentialService.GetCredential(principal, Options);
            if (success)
            {
                principal.CurrentRole = principal.Roles[0];
                credential += "a";
                return (success, msg, credential, principal);
            }
            else
            {
                return (success, msg, credential, Anonymous);
            }
        }

        public IUserPrincipal? GetUserPrinciple(string credential)
        {
            if (!credential.HasValue()) return null;

            char indexChar = credential.Last();
            int roleIndex = indexChar - 'a';

            credential = credential[0..^1];

            UserPrincipal? user = credentialService.GetUser(credential) as UserPrincipal;
            if (user != null)
            {
                if (user.Roles.Length == 0)
                {
                    user.Roles = [AnnoymousRole];
                }

                foreach (var item in user.Roles.Where(e => e.UseAsPrinciple))
                {
                    RoleSchema? schema = roleService.GetRoleSchemaById(item.ID);
                    if (schema != null)
                    {
                        item.Menus = schema.FinalMenus;
                        item.Permissions = schema.FinalPermissions;
                    }
                }

                roleIndex = roleIndex >= user.Roles.Length ? 0 : roleIndex;
                user.CurrentRole = user.Roles[roleIndex];
            }

            return user;
        }

        public void Logout(string credential)
        {
            try
            {
                credentialService.RemoveCredential(credential[0..^1]);
            }
            catch
            {

            }
        }

        public void RemoveUser(string code)
        {
            credentialService.RemoveUser(code);
        }
    }
}
