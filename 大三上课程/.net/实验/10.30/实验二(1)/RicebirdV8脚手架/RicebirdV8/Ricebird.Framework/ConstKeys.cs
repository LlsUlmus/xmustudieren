namespace Ricebird.Framework
{
    public static class ConstKeys
    {
        public const string CorsAny = "Any";
        public const string AuthenticationKey = "access_token";
        public const string AuthorizeClaim = "_authorize_";
        /// <summary>
        /// 针对数据库的最小日期
        /// </summary>
        public static readonly DateTime MinDate = new DateTime(1970, 1, 1);
        /// <summary>
        /// 针对数据库的最大日期
        /// </summary>
        public static readonly DateTime MaxDate = new DateTime(2099, 1, 1);
        public const string PasswordNotSet = "vjtEt$%SNfXzpBXt";
        public const string IUserIdentityHasChangedFlag = "HasChanged";
        public const string IUserRemoveFlag = "HasRemoved";
    }

    public static class RateLimitPolicyKeys
    {
        public const string 按用户限流 = "by_user";
    }
}
