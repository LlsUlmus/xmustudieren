using Ricebird.Framework.Security.Apis;

namespace Ricebird.Framework
{
    public abstract class WebModule
    {
        /// <summary>
        /// 用以标识的英文名
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 优先级 数字越高，载入顺序越靠后
        /// </summary>
        public abstract int Priority
        {
            get;
        }

        /// <summary>
        /// 用以显示的中文名
        /// </summary>
        public abstract string DisplayName
        {
            get;
        }

        public virtual List<ApiDescriptor> Apis
        {
            get; set;
        } = [];

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public HostEnv HostEnv { get; set; }
        public Assembly Assembly { get; internal set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public abstract void Register(IServiceCollection services);

        public abstract void Use(WebApplication app);
    }
}
