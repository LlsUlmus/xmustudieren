namespace Ricebird.Framework.Security
{
    /// <summary>
    /// 这个接口的对象保存在 hostEnv.CredentialService 中，不能使用Resolve
    /// </summary>
    public interface ICredentialService
    {
        void UpdateUser(CommonUser user);

        int TotalToken { get; }
        int TotalUser { get; }
    }
}
