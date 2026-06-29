namespace Ricebird.Framework.Configurations
{
    /// <summary>
    /// 选项的持久化场所
    /// </summary>
    public enum OptionSaveTo
    {
        /// <summary>
        /// 将选项保存在文件系统中
        /// </summary>
        FileSystem,
        /// <summary>
        /// 将选项保存在数据库中
        /// </summary>
        Database,
        User,
        Department
    }

    [Flags]
    public enum ConfigOwner : long
    {
        /// <summary>
        /// 任何一种
        /// </summary>
        Any = 65535,
        /// <summary>
        /// 所有
        /// </summary>
        All = 1,
        /// <summary>
        /// 系统
        /// </summary>
        System = 2,
        /// <summary>
        /// 用户独享
        /// </summary>
        User = 4
    }
}
