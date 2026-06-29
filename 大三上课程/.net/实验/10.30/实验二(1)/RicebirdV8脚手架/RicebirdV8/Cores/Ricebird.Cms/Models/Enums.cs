namespace Ricebird.Cms.Models
{
    public enum CategoryType
    {
        Normal = 0,
        Link = 1,
        Gallery = 2,
        Timeline = 3,
        Home = 4
    }

    public enum Visibility
    {
        Visiable = 0,
        Hidden = 1
    }

    public enum CommentStatus
    {
        Disable = 0,
        Enable = 1,
    }

    /// <summary>
    /// 新闻的审核状态
    /// </summary>
    public enum VerifyStatus
    {
        All = -1,
        /// <summary>
        /// 未审核
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// 已经审核
        /// </summary>
        Pass = 1,
        /// <summary>
        /// 撤稿
        /// </summary>
        Deny = 2,
        /// <summary>
        /// 等待自动发布
        /// </summary>
        WaitPass = 3,
    }

    [Flags]
    public enum TopMostType
    {
        Normal,
        TopMost,
        SuperTop
    }
}
