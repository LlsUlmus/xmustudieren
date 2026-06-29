using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;

namespace Ricebird.Framework.AspNetCoreExtensions.Filters
{
    public class ExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var provider = context.HttpContext.RequestServices;
            var client = provider.Resolve<IClient>();
            var env = provider.Resolve<HostEnv>();
            var opt = RicebirdSerializerOption.Default;
            opt.WriteIndented = true;

            Exception? ex = context.Exception;
            StringBuilder builder = new StringBuilder();
            try
            {
                builder.AppendLine(ex.Message);
                builder.AppendLine(ex.StackTrace);
                builder.AppendLine("----------------------");
                builder.AppendLine("jsonObj:");
                builder.AppendLine(ex.SearializeJson());
            }
            catch
            {
                builder = new StringBuilder();
                ex = context.Exception;
                while (ex != null)
                {
                    builder.AppendLine(ex.Message);
                    builder.AppendLine(ex.StackTrace);
                    builder.AppendLine("----------------------");
                    ex = ex.InnerException;
                }
            }

            builder.AppendLine("----------------------");
            builder.AppendLine("QueryString");
            builder.AppendLine(context.HttpContext.Request.QueryString.ToString());
            builder.AppendLine("----------------------");
            builder.AppendLine("POST参数");
            builder.AppendLine(client.PostStream);

            env.WriteLog("[未处理异常]", builder.ToString());
            client.LogException(context.Exception, "米雀异常", context.ActionDescriptor.DisplayName ?? "");

            var result = new RicebirdJsonResult(new
            {
                success = false,
                msg = context.Exception.Message,
                ip = client.RealIp
            }, true, "yyyy年M月d日");

            context.ExceptionHandled = true;
            context.Result = result;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }
    }
}
