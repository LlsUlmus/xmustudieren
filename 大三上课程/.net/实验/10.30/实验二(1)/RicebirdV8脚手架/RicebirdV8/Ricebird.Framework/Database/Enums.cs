namespace Ricebird.Framework.Database
{
    /// <summary>
    /// 将要进行的数据库操作
    /// </summary>
    public enum DbOperate : int
    {
        /// <summary>
        /// 添加操作
        /// </summary>
        Create,
        /// <summary>
        /// 修改操作
        /// </summary>
        Update,
        /// <summary>
        /// 查询操作
        /// </summary>
        Search,
        /// <summary>
        /// 删除操作
        /// </summary>
        Remove,
        /// <summary>
        /// 没有进行任何操作
        /// </summary>
        None
    }

    public enum EntityStatus
    {
        Exists,
        New,
        Unknown
    }

    public static class EnumToStringHelper
    {
        public static string GetText(this DbOperate operate) => operate switch
        {
            DbOperate.Create => "创建",
            DbOperate.Update => "修改",
            DbOperate.Search => "查找",
            DbOperate.Remove => "删除",
            _ => string.Empty,
        };
    }
}
