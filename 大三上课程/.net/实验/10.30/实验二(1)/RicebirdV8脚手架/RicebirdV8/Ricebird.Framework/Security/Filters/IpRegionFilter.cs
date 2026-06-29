using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;

namespace Ricebird.Framework.Security
{
    public class IpRegion(string name) : ActionFilterAttribute
    {
        public string Name => name;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IClient client = context.HttpContext.RequestServices.Resolve<IClient>();
            if (!client.IpRegions.Contains(Name))
            {
                context.Result = new RicebirdJsonResult(new
                {
                    success = false,
                    code = "403",
                    msg = $"必须在{Name}才能访问此接口",
                    client.RealIp,
                    page = 1,
                    pageSize = 10,
                    totalRow = 0,
                    data = new List<string>()
                }, true, "yyyy-M-d");
                return;
            }
        }
    }
}
