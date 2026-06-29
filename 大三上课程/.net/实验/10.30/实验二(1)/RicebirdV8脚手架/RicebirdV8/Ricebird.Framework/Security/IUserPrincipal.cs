using System.Security.Claims;

namespace Ricebird.Framework.Security
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public interface IUserPrincipal
    {
        Guid ID { get; }
        /// <summary>
        /// 有效证件号
        /// </summary>
        string Code { get; }
        /// <summary>
        /// 姓名
        /// </summary>
        string RealName { get; }
        string Mobile { get; }
        string Email { get; }
        /// <summary>
        /// 头像
        /// </summary>
        string Avatar { get; }
        /// <summary>
        /// 所属部门
        /// </summary>
        Guid RootDepartId { get; }
        /// <summary>
        /// 用户状态
        /// </summary>
        UserStatus AuditStatus { get; }

        string[] IpRegions { get; }

        string OpenId { get; }

        bool IsAnonymous => ID == Guid.Empty;

        [JsonIgnore]
        Dictionary<string, object> Cliams
        {
            get;
        }

        Role[] Roles { get; }

        [JsonIgnore]
        Role CurrentRole
        {
            get;
        }

        bool Succeed(string permission);
        bool Succeed(IEnumerable<string> permissions);
        bool Succeed(params string[] permissions);
        bool IsInRoles(params string[] roleName);
        bool TryGetRole(string roleName, [NotNullWhen(true)] out Role? role);

        ClaimsPrincipal GetClaimsPrincipal(string token);
    }
}
