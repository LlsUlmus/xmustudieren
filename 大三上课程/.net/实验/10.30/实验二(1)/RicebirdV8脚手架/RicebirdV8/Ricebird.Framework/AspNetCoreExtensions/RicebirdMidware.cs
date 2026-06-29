namespace Ricebird.Framework.AspNetCoreExtensions
{
    /// <summary>
    /// 用于构建HttpContext的中间件
    /// </summary>
    public abstract class RicebirdMidware(RequestDelegate next)
    {
        protected readonly RequestDelegate _next = next;

        public abstract Task Invoke(HttpContext context);

        protected T EnsureInitialize<T>(HttpContext ctx, string service, string dependency)
        {
            T? feature = ctx.Features.Get<T>();
            return feature == null ? throw new NotSupportedException($"必须在 {dependency} 初始化之后，方可支持{service}。") : feature;
        }
    }
}
