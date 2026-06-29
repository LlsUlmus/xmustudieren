namespace Ricebird.Cms.DataSources
{
    /// <summary>
    /// 内容缓存的过期策略
    /// </summary>
    public enum ExpireStrategy
    {
        /// <summary>
        /// 绝对时间过期
        /// </summary>
        Absolute,
        /// <summary>
        /// 滑动时间过期
        /// </summary>
        Sliding
    }

    /// <summary>
    /// 到期后的清理策略
    /// </summary>
    public enum ClearStrategy
    {
        /// <summary>
        /// 清除缓存
        /// </summary>
        ClearCache,
        /// <summary>
        /// 删除自身
        /// </summary>
        RemoveSelf
    }
}
