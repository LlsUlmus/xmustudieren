namespace Ricebird.Security.ViewModels
{
    internal class AuthCredential(IUserPrincipal entity, TimeSpan expiredIn)
    {
        public IUserPrincipal UserPrincipal { get; set; } = entity;

        public string Credential { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".GenerateKey(8);

        public TimeSpan ExpiredIn { get; set; } = expiredIn;

        internal DateTime LastAccess { get; set; } = DateTime.Now;

        internal bool CheckExpired(DateTime now)
        {
            return (now - LastAccess) >= ExpiredIn;
        }
    }
}
