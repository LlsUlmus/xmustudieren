using Ricebird.Framework.Security;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Ricebird.Clients.Models
{
    [DontAutoRegistion]
    public record SystemUserEntity(Guid ServiceKey, string ServiceName) : IUserPrincipal
    {
        public Guid ID => ServiceKey;

        public string RealName => ServiceName;

        public string UserName => ServiceName;

        public string Mobile => string.Empty;

        public string Email => string.Empty;

        public string Avatar => "";

        public string UserType => "";

        public AccessLevel Level => AccessLevel.Max;

        public string Code => "System";

        public UserStatus AuditStatus => UserStatus.Enable;

        public Guid RootDepartId => Guid.Empty;

        public Dictionary<string, object> Cliams => [];

        public Role[] Roles => [];

        public Role CurrentRole => new Role() { Name = "超级管理员" };

        public string OpenId => string.Empty;

        public string[] IpRegions => [];

        public bool IsInRoles(params string[] roleName) => true;
        public bool Succeed(string permission) => true;
        public bool Succeed(IEnumerable<string> permissions) => true;

        public bool TryGetRole(string roleName, [NotNullWhen(true)] out Role? role)
        {
            role = null;
            return false;
        }

        public bool Succeed(params string[] permissions) => true;

        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            List<Claim> claims = [new Claim(nameof(ID), ID.ToString()), new Claim(nameof(RealName), RealName)];
            ClaimsIdentity identity = new ClaimsIdentity(claims, "系统用户");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            return principal;
        }

        public override string ToString() => "系统";
    }
}
