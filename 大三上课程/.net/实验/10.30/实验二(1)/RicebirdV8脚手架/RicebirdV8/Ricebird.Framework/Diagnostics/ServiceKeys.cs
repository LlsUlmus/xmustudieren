namespace Ricebird.Framework.Diagnostics
{
    public static class ServiceKeys
    {
        /// <summary>
        /// 代指本系统用的ID
        /// </summary>
        public static readonly Guid FrameworkKey = new Guid("81c0b1e8-14aa-4991-97e4-49895e017c45");
        /// <summary>
        /// 任务模块用的ID
        /// </summary>
        public static readonly Guid TaskServiceKey = new Guid("97a8c064-c914-42a4-8a1a-b6f336d7a28d");
        /// <summary>
        /// 授权模块用的ID
        /// </summary>
        public static readonly Guid AuthorizationServiceKey = new Guid("85dc9d60-1611-4b74-9a13-6f9920f28b6d");
        /// <summary>
        /// 身份验证模块用的ID
        /// </summary>
        public static readonly Guid AuthenticationServiceKey = new Guid("475d4b76-b990-4a3e-80ba-faead0c4ae72");
        /// <summary>
        /// 许可证模块用的ID
        /// </summary>
        public static readonly Guid IdentityService = new Guid("2385bc6a-e63f-4eb9-a17e-5c45203675bc");
        /// <summary>
        /// 缓存模块
        /// </summary>
        public static readonly Guid MemeryCacheService = new Guid("e29855ca-f4f9-4739-bb1c-8eea204a2389");
        /// <summary>
        /// 权限验证模块
        /// </summary>
        public static readonly Guid PermissionProviderServiceKey = new Guid("1824011f-e096-446d-be03-19494783055e");
        /// <summary>
        /// 消息模块用的ID
        /// </summary>
        public static readonly Guid SmsServiceKey = new Guid("d5bf2626-d2ed-436e-87be-8cd8d738f2c7");
        /// <summary>
        /// 短地址服务用的ID
        /// </summary>
        public static readonly Guid UrlServiceKey = new Guid("8b2110e1-13a7-4f7b-ac58-03146c8b19ec");
        /// <summary>
        /// 令牌服务用的ID
        /// </summary>
        public static readonly Guid TokenServiceKey = new Guid("d61264a9-abe7-4cd9-ad52-c87ca00aee10");
        /// <summary>
        /// 配置服务用的ID
        /// </summary>
        public static readonly Guid ConfigServiceKey = new Guid("9849fb17-5b4e-4299-9d39-2803e413b972");
        /// <summary>
        /// 客户端服务ID
        /// </summary>
        public static readonly Guid ClientSericeKey = new Guid("CD545492-3427-2AAA-950F-C14B17990CBE");
    }
}
