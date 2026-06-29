using Ricebird.Framework.AspNetCoreExtensions;
using Ricebird.Framework.Diagnostics.Features;

namespace Ricebird.Diagnostics.Midwares
{
    internal class DiagnosticMidware(RequestDelegate next) : RicebirdMidware(next)
    {
        public override async Task Invoke(HttpContext context)
        {
            var Services = context.RequestServices;
            var env = Services.Resolve<HostEnv>();
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "无IP";
            string method = context.Request.Method;
            string url = context.Request.Path;

            Browser? browser = context.Features.Get<Browser>();
            if (browser == null)
            {
                browser = new Browser(context);
                context.Features.Set(browser);
            }

            ConnectLog log = new ConnectLog(browser, ip, method, url);
            bool hasException = false;
            try
            {
                await _next(context);
            }
            catch
            {
                hasException = true;
                throw;
            }
            finally
            {
                int statusCode = hasException ? 500 : context.Response.StatusCode;
                log.End(statusCode);
                if (env.ShouldLog())
                {
                    //var ctx = Services.Resolve<RepositoryBase<ConnectLog>>();
                    //await ctx.SaveChangesAsync();
                }
            }
        }
    }
}
