using Microsoft.AspNetCore.Mvc.Filters;

namespace Ricebird.Framework
{
    public class DebugController : ApiController
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HostEnv env = context.HttpContext.RequestServices.Resolve<HostEnv>();
            if (env.IsProduction())
            {
                context.Result = Fail("禁止访问此类型的端口");
                context.HttpContext.Response.StatusCode = 403;
                // context.Result = NotFound();
            }
        }
    }
}
