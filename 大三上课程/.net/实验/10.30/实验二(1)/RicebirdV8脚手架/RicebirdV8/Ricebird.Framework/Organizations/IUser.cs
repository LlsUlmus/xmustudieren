using Ricebird.Framework.Security;

namespace Ricebird.Framework.Organizations
{
    public interface IUser
    {
        Guid ID { get; set; }
        UserStatus AuditStatus { get; set; }
        string Avatar { get; set; }
        string Code { get; set; }
        string CodeType { get; set; }
        int DisplayOrder { get; set; }
        string Email { get; set; }
        AccessLevel Level { get; set; }
        int LockCount { get; set; }
        DateTime LockTo { get; set; }
        string Mobile { get; set; }
        string OpenId { get; set; }
        string RealName { get; set; }
        Guid RootDepartId { get; set; }
        string UserName { get; set; }
        string UserPassword { get; set; }
        string UserSource { get; set; }
    }
}
