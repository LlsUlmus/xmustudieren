namespace Ricebird.Framework.Security
{
    #region 用户的枚举
    [DataDictionary("用户状态")]
    public enum UserStatus
    {
        /// <summary>
        /// 禁用
        /// </summary>
        [DataEntry("禁用")]
        Disable = 0,
        /// <summary>
        /// 启用
        /// </summary>
        [DataEntry("启用")]
        Enable = 1,
        /// <summary>
        /// 用户被锁定
        /// </summary>
        [DataEntry("锁定")]
        IsLockout = 2,
        /// <summary>
        /// 账号需要验证码
        /// </summary>
        [DataEntry("下次登录时修改密码", Enable = false, Visible = false)]
        MustChangePassword = 3,
    }

    /// <summary>
    /// 访问权限设置
    /// </summary>
    [DataDictionary("访问权限")]
    public enum AccessLevel
    {
        /// <summary>
        /// 全部允许
        /// </summary>
        [DataEntry("超级管理员")]
        AllAccess = 0,
        /// <summary>
        /// 全部拒绝（某些特定用户除外）
        /// </summary>
        [DataEntry("完全禁止", Enable = false, Visible = false)]
        AllDeny = 1,
        /// <summary>
        /// 访问权限最小化
        /// </summary>
        [DataEntry("最小化权限", Enable = false, Visible = false)]
        Min = 2,
        /// <summary>
        /// 访问权限最大化
        /// </summary>
        [DataEntry("按角色设置")]
        Max = 3
    }
    #endregion

    #region 角色的枚举
    /// <summary>
    /// 角色的适用范围
    /// </summary>
    [DataDictionary("角色适用范围")]
    public enum RuleFor
    {
        /// <summary>
        /// 任意类型
        /// </summary>
        [DataEntry("任意")]
        Any = -1,
        /// <summary>
        /// 用户
        /// </summary>
        [DataEntry("用户")]
        User = 0,
        /// <summary>
        /// 部门
        /// </summary>
        [DataEntry("部门")]
        Department = 1,
    }

    /// <summary>
    /// 权限级别
    /// </summary>
    [DataDictionary("可访问性")]
    public enum AuthorizeResult
    {
        /// <summary>
        /// 未设置
        /// </summary>
        [DataEntry("未设置")]
        NoSet = -1,
        /// <summary>
        /// 允许访问
        /// </summary>
        [DataEntry("允许")]
        Access = 0,
        /// <summary>
        /// 拒绝
        /// </summary>
        [DataEntry("拒绝")]
        Deny = 1,
    }
    #endregion

    #region 权限的枚举
    [DataDictionary("接口类型")]
    public enum ApiType
    {
        [DataEntry("所有类型")]
        All = -1,
        [DataEntry("内置模块")]
        Module = 0,
        [DataEntry("网络接口")]
        WebApi = 1,
        [DataEntry("系统功能")]
        Permission = 2,
    }

    [DataDictionary("授权等级")]
    public enum ApiAuthorizeLevel
    {
        [DataEntry("任意等级")]
        All = -1,
        [DataEntry("无需授权")]
        None = 0,
        [DataEntry("仅需登录")]
        Login = 1,
        [DataEntry("代码中验证")]
        AuthorizeInCode = 2,
        [DataEntry("必须授权")]
        Authorize = 3,
        [DataEntry("链接权限")]
        LinkToOther = 4,
    }

    [DataDictionary("接口状态")]
    public enum ApiStatus
    {
        [DataEntry("无")]
        None,
        [DataEntry("在线")]
        Connecting,
        [DataEntry("离线")]
        Disconnecting,
        [DataEntry("废弃")]
        Obsoleted,
    }

    public enum ApiResult
    {
        Success = 0,
        Failure = 1,
        Exception = 2,
    }
    #endregion
}
