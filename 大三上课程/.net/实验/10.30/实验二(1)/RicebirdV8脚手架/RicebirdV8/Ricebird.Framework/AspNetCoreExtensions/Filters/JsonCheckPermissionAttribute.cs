using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;

namespace Ricebird.Framework
{
    public class JsonCheckPermissionAttribute : ActionFilterAttribute
    {
        public List<string> Permissions { get; set; } = [];

        public JsonCheckPermissionAttribute(string permission) => Permissions.Add(permission);

        /// <summary>
        /// 任意一个权限满足，即可
        /// </summary>
        /// <param name="permissions"></param>
        public JsonCheckPermissionAttribute(params string[] permissions) => Permissions.AddRange(permissions);

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
                    msg = "你必须先登录才能访问本接口",
                    page = 1,
                    pageSize = 10,
                    totalRow = 0,
                    data = new List<string>()
                }, true, "");
                return;
            }

            if (!client.Successed(Permissions))
            {
                context.Result = new RicebirdJsonResult(new
                {
                    success = false,
                    msg = "你没有访问此接口的权限",
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
