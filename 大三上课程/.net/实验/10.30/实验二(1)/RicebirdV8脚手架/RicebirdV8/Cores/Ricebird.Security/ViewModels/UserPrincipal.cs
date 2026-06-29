using Ricebird.Framework.Organizations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Ricebird.Security.ViewModels
{
    public class UserPrincipal : IUserPrincipal
    {
        public UserPrincipal()
        {
            Roles = [];
            CurrentRole = new Role();
        }

        public UserPrincipal(IUser u)
        {
            ID = u.ID;
            Code = u.Code;
            RealName = u.RealName;
            Avatar = u.Avatar;
            RootDepartId = u.RootDepartId;
            OpenId = u.OpenId;
            Roles = [];
            CurrentRole = new Role();
        }

        internal UserPrincipal(Role currentRole, Role[] roles)
        {
            Roles = roles;
            CurrentRole = currentRole;
        }

        internal UserPrincipal(Role[] roles)
        {
            Roles = roles;
            CurrentRole = roles[0];
        }

        #region 字段
        /// <summary>
        /// 用户的ID
        /// </summary>
        public Guid ID
        {
            get; set;
        } = Guid.Empty;

        public string Code
        {
            get; set;
        } = string.Empty;

        public string RealName
        {
            get; set;
        } = "匿名用户";

        public string Mobile
        {
            get;set;
        } = string.Empty;

        public string Email
        {
            get; set;
        } = string.Empty;

        public string Avatar
        {
            get; set;
        } = string.Empty;

        public string[] IpRegions
        {
            get; set;
        } = [];

        [JsonIgnore]
        public Guid RootDepartId
        {
            get; set;
        } = Guid.Empty;

        [JsonIgnore]
        public UserStatus AuditStatus
        {
            get; set;
        } = UserStatus.Disable;

        [JsonIgnore]
        public string OpenId
        {
            get; set;
        } = string.Empty;

        [JsonIgnore]
        public Dictionary<string, object> Cliams
        {
            get; set;
        } = [];
        #endregion

        public Role[] Roles { get; set; }

        [JsonIgnore]
        public Role CurrentRole
        {
            get; set;
        }

        public bool IsInRoles(params string[] roleName) => Roles.Any(e => roleName.Any(x => x == e.Name));

        public bool TryGetRole(string roleName, [NotNullWhen(true)] out Role? role)
        {
            role = Roles.FirstOrDefault(e => e.Name == roleName);
            return role != null;
        }

        public bool Succeed(string permission)
        {
            return CurrentRole.Permissions.Contains(permission) || Roles.Any(x => x.Permissions.Contains(permission));
        }

        public bool Succeed(IEnumerable<string> permissions)
        {
            return permissions.Any(Succeed);
        }

        public bool Succeed(params string[] permissions)
        {
            return Succeed(permissions as IEnumerable<string>);
        }

        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            List<Claim> claims = [new Claim(nameof(ID), ID.ToString()), new Claim(nameof(RealName), RealName), new Claim("Token", token)];
            ClaimsIdentity identity = new ClaimsIdentity(claims, "普通用户");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            return principal;
        }

        public override string ToString() => Code.HasValue() ? $"{RealName}/{Code}" : $"{RealName}";
    }
}
