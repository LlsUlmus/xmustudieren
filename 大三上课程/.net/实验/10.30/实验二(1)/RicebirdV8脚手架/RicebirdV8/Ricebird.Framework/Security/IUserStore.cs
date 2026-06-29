namespace Ricebird.Framework.Security
{
    public interface IUserStore
    {
        CommonUser GetUser(string token);

        CommonUser GetUserByCode(string code);

        List<(Guid roleId, Guid departId, string departName)> GetRelations(Guid userId);
    }

    public record CommonUser(Guid ID, string RealName, string Avatar, string Code, string Mobile, string Email, UserStatus AuditStatus, string Password, AccessLevel Level, string OpenId, Guid RootDepartId);
}
