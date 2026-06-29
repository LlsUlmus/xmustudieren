namespace Ricebird.Framework.ShortUrl
{
    public enum UrlState : int
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 临时
        /// </summary>
        Temporary = 1,
        /// <summary>
        /// 永久
        /// </summary>
        Permanent = 2,
        /// <summary>
        /// 一次性
        /// </summary>
        Once = 4
    }
}
