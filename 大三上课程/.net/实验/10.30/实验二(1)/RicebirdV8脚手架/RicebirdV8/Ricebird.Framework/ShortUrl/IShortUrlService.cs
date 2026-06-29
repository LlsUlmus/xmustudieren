using Ricebird.Framework.Clients;

namespace Ricebird.Framework.ShortUrl
{
    public interface IShortUrlService : ISingletonDependency
    {
        string ToLongUrl(string code);

        /// <summary>
        /// 该函数返回的Code只能一次性使用
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string ToShortUrlCode(string url);

        /// <summary>
        /// 该函数返回的Code可以多次使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        string ToShortUrlCode(IClient client, string url, TimeSpan duration);

        /// <summary>
        /// 该函数返回的Code可以多次使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        string ToPermanentUrlCode(IClient client, string url);

        /// <summary>
        /// 该函数返回的Url只能一次性使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        string ToShortUrl(IClient client, string url);

        /// <summary>
        /// 该函数返回的Url可以多次使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        string ToShortUrl(IClient client, string url, TimeSpan duration);

        /// <summary>
        /// 该函数返回的Url可以多次使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        string ToPermanentUrl(IClient client, string url);
    }
}