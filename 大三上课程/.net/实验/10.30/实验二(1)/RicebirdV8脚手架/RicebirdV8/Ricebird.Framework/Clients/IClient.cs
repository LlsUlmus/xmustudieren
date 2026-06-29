using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ricebird.Framework.Database;
using Ricebird.Framework.Diagnostics;
using Ricebird.Framework.Diagnostics.Features;
using Ricebird.Framework.Security;

namespace Ricebird.Framework.Clients
{
    public interface IClient : IEquatable<IClient>, IScopedDependency
    {
        Guid Id { get; }

        ClientType Type { get; set; }

        void BuildClient(IServiceScope scope, Guid sKey, string sName);

        #region 读取选项
        T LoadOptions<T>() where T : IOption, new();
        #endregion

        #region 取参数
        string PostStream
        {
            get;
        }

        /// <summary>
        /// 向请求中注入数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data">该数据为最高优先级。只要这里有的数据，就会优先取用。</param>
        void MergeData(string key, object data);

        bool TryGet<T>(string paramName, T defaultValue, [NotNullWhen(true)] out T? value);

        bool TryGet(string paramName, Type valueType, [NotNullWhen(true)] out object? value);

        /// <summary>
        /// 从除Cookie以外的地方取参数
        /// <para>
        /// 这种方法取的参数可以防止 CSRF 攻击
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetInRequest<T>(string paramName, T defaultValue);

        /// <summary>
        /// 从包括Cookie在内的地方取参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T Get<T>(string paramName, T defaultValue);

        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        List<T> GetList<T>(string paramName, string seperator);

        /// <summary>
        /// 从包括Cookie在内的地方取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void FillObject<T>(T obj);

        /// <summary>
        /// 从包括Cookie在内的地方取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void FillObject<T>(T obj, params string[] ignoreProperties);

        TEntity FillResolveObject<TEntity>()
             where TEntity : class;

        /// <summary>
        /// 从输入中获取对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="ignoreProperties">忽略的字段</param>
        /// <returns></returns>
        TEntity FillResolveObject<TEntity>(params string[] ignoreProperties)
            where TEntity : class;

        ModelBindingResult BindEntity(Type entityType);

        TEntity? Deserialize<TEntity>();
        #endregion

        #region 客户端相关的信息
        string RealIp
        {
            get;
        }

        string UserAgent
        {
            get;
        }

        bool IsWeixinClientRequest { get; }

        /// <summary>
        /// 获取显示的URL, 取完整的URL，不带QueryString
        /// <para>
        /// https://www.ricebird.cn:7070/api/test?id=3 -> https://www.ricebird.cn:7070/api/test
        /// </para>
        /// </summary>
        string DisplayUrl { get; }

        /// <summary>
        /// 获取Url中地址部分，不带不带QueryString
        /// <para>
        /// https://www.ricebird.cn:7070/api/test?id=3 -> /api/test
        /// </para>
        /// </summary>
        string ApiPath
        {
            get;
        }

        public string Method
        {
            get;
        }

        /// <summary>
        /// 显示网站的根域名，没有最后的 /
        /// <para>
        /// https://www.ricebird.cn:7070/api/test?id=3 -> https://www.ricebird.cn:7070
        /// </para>
        /// </summary>
        string HostWithScheme { get; }

        List<string> IpRegions { get; }

        /// <summary>
        /// 获取当前页面的所有参数
        /// </summary>
        string Params { get; }
        #endregion

        #region 当前用户相关
        IUserPrincipal CurrentUser { get; }

        bool Successed(string permission);

        /// <summary>
        /// 任意一个权限满足，即可
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool Successed(IEnumerable<string> permission);
        #endregion

        #region 服务相关内容
        HttpContext? HttpContext
        {
            get;
        }

        HttpRequest? Request
        {
            get;
        }

        IServiceProvider Services
        {
            get;
        }

        IFeatureCollection Features
        {
            get;
        }

        Browser Browser
        {
            get;
        }

        T Resolve<T>()
            where T : class;

        T Resolve<T>(string productName)
            where T : class;
        #endregion

        #region Cookie相关函数
        /// <summary>
        /// 设置本地cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>  
        /// <param name="minutes">过期时长，单位：分钟</param>      
        [Obsolete("已过期，请使用localStorage保存数据")]
        void SetCookies(string key, string value, int minutes = 30);

        /// <summary>
        /// 删除指定的cookie
        /// </summary>
        /// <param name="key">键</param>
        [Obsolete("已过期，请使用localStorage保存数据")]
        void DeleteCookies(string key);

        /// <summary>
        /// 获取cookies
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        [Obsolete("已过期，请使用localStorage保存数据")]
        string GetCookies(string key);
        #endregion

        #region 日志接口
        IDbLogger Logger { get; }
        void LogException(Exception ex, string module, string method);
        void Log(string module, string method, string relateId, string desc);
        void Log(string module, string method, Guid relateId, string desc) => Log(module, method, relateId.ToString(), desc);
        void Log(string module, string method, EntityBase? entity, string desc) => Log(module, method, (entity?.ID ?? Guid.Empty).ToString(), desc);
        #endregion

        #region 复制一个新的Client
        IClient Clone(IServiceScope scope, Guid sKey, string sName);
        void Dispose();
        #endregion
    }
}
