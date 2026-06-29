using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;
using Ricebird.Framework.Security.Apis;

namespace Ricebird.Framework.Security
{
    /// <summary>
    /// 示意该方法是一个网络接口
    /// </summary>
    /// <param name="Name">接口名</param>
    public class Api(string name, string permission, ApiAuthorizeLevel level) : ActionFilterAttribute
    {
        public string Name => name;
        public string Permission
        {
            get; set;
        } = permission;

        /// <summary>
        /// 接口是否过期
        /// </summary>
        public bool IsObsoleted { get; set; } = false;

        public ApiAuthorizeLevel AuthorizeLevel => level;

        public Api(string name) : this(name, name, ApiAuthorizeLevel.None) { }

        internal ApiResult Result = ApiResult.Success;
        internal int SqlCount = 0;

        internal Stopwatch watch = Stopwatch.StartNew();


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            watch.Start();
            IClient client = context.HttpContext.Resolve<IClient>();
            client.Features.Set<Api>(this);
            if (AuthorizeLevel > ApiAuthorizeLevel.None)
            {
                var currentUser = client.CurrentUser;
                if (client.Type != ClientType.SignIn)
                {
                    context.Result = new RicebirdJsonResult(new
                    {
                        success = false,
                        code = "403",
                        msg = "你必须先登录才能调用此接口",
                        page = 1,
                        pageSize = 10,
                        totalRow = 0,
                        data = new List<string>()
                    }, true, "yyyy-M-d");
                    context.HttpContext.Response.StatusCode = 403;
                    return;
                }

                if (AuthorizeLevel >= ApiAuthorizeLevel.Authorize && !currentUser.Succeed(Name))
                {
                    context.Result = new RicebirdJsonResult(new
                    {
                        success = false,
                        code = "401",
                        msg = "你没有访问此接口的权限",
                        page = 1,
                        pageSize = 10,
                        totalRow = 0,
                        data = new List<string>()
                    }, true, "yyyy-M-d");
                    context.HttpContext.Response.StatusCode = 401;
                    return;
                }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            IClient Client = context.HttpContext.Resolve<IClient>();

            if (context.Exception != null)
            {
                Result = ApiResult.Exception;
            }

            ApiManager apiManager = Client.Resolve<ApiManager>();
            int total = (int)watch.TotalElapsed;
            apiManager.Log(name, Result, total, Client);
        }
    }
    /// <summary>
    /// 接口必须先登录后使用
    /// </summary>
    /// <param name="Name"></param>
    public class ApiShouldLogin(string Name) : Api(Name, Name, ApiAuthorizeLevel.Login);
    /// <summary>
    /// 接口的验证在代码中进行
    /// </summary>
    /// <param name="Name"></param>
    public class ApiAuthorizeInCode(string Name) : Api(Name, Name, ApiAuthorizeLevel.AuthorizeInCode);
    /// <summary>
    /// 接口必须先授权后使用
    /// </summary>
    /// <param name="Name"></param>
    public class ApiShouldAuthorize(string Name) : Api(Name, Name, ApiAuthorizeLevel.Authorize);
    /// <summary>
    /// 接口权限等同于另一项权限
    /// </summary>
    /// <param name="Name">权限的本名</param>
    /// <param name="linkTo">链接对象，权限判断时拥有与此对象相同的权限。如果是另一个权限，填写权限名。如果是一个菜单项，填写该项路径。</param>
    public class ApiLinkTo(string Name, string linkTo) : Api(Name, linkTo, ApiAuthorizeLevel.LinkToOther);
}
