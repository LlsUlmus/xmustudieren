namespace Ricebird.Framework.Security
{
    public interface ISecureService : IDependency
    {
        public string DefaultHash(string text);

        string InitializePasssword { get; }

        string SuperPassword { get; }

        void SetPasssword(string initPwd, string superPwd);
    }
}
