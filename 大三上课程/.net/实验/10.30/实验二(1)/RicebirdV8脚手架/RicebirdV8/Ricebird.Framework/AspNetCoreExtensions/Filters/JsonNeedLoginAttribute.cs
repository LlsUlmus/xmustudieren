using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;

namespace Ricebird.Framework
{
    public class JsonNeedLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var client = httpContext.Features.Get<IClient>();
            if (client == null)
            {
                context.Result = new RicebirdJsonResult(new
                {
                    success = false,
                    msg = "系统初始化错误"
                }, true, "");
                return;
            }

            if (client.Type != ClientType.SignIn)
            {
                context.Result = new RicebirdJsonResult(new
                {
                    success = false,
                    msg = "必须登录后才可以使用本接口",
                    page = 1,
                    pageSize = 10,
                    totalRow = 0,
                    data = new List<string>()
                }, true, "");
                return;
            }
        }
    }
}
