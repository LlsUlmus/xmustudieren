using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Ricebird.Framework.Database;
using Ricebird.Framework.DataValidator;
using Ricebird.Framework.FileStorage;
using Ricebird.Framework.Security;
using Ricebird.Framework.Tools;

namespace Ricebird.Framework
{
    public partial class ApiController : RicebirdController
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HostEnv env = Resolve<HostEnv>();

            var getAttr = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.GetCustomAttribute<HttpGetAttribute>();
            if (context.HttpContext.Request.Method == "GET" && !env.IsDevelopment() && !env.FrameworkOptions.AlwaysAllowGet && getAttr == null)
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    msg = "无法通过GET访问此接口。"
                });
            }

            base.OnActionExecuting(context);
        }

        protected virtual ActionResult JsonString(string json) => Content(json, "application/json");

        protected new virtual ActionResult Ok()
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Success;
            }

            return RJson(new
            {
                success = true,
                msg = "操作成功"
            });
        }

        protected virtual ActionResult Ok(object obj, string dateTimeFormat = "yyyy年M月d日")
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Success;
            }
            return RJson(obj, dateTimeFormat);
        }

        protected virtual ActionResult Ok(string msg)
        {
            return Ok(new
            {
                success = true,
                msg
            });
        }

        protected virtual ActionResult Fail()
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Failure;
            }

            return RJson(new
            {
                success = false,
                msg = "操作失败"
            });
        }

        protected virtual ActionResult Fail(object obj, string dateTimeFormat = "yyyy年M月d日")
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Failure;
            }
            return RJson(obj, dateTimeFormat);
        }

        protected ActionResult Fail(string msg)
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Failure;
            }
            return RJson(new
            {
                success = false,
                msg,
                errorStrings = new string[] { msg }
            });
        }

        protected ActionResult Fail(string msg, params string[] details)
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Failure;
            }
            return RJson(new
            {
                success = false,
                msg,
                errorStrings = details
            });
        }

        protected ActionResult FailPage(string msg)
        {
            var api = Client.Features.Get<Api>();
            if (api != null)
            {
                api.Result = ApiResult.Failure;
            }

            return ResultPages.ResultPage(ResultPages.WARNING, msg);
        }

        protected virtual ActionResult ValidateError(ValidateResult result)
        {
            return Fail(new
            {
                success = false,
                msg = result.ErrorStrings,
                errors = result.Errors,
                errorStrings = result.ErrorStrings
            });
        }

        public ActionResult File((string msg, IFile? file) ans)
        {
            if (ans.file == null)
            {
                return Fail(ans.msg);
            }

            return Ok(new
            {
                success = true,
                ans.msg,
                download = ans.file.DownloadPath,
                id = ans.file.UniqueCode
            });
        }

        public ActionResult FilePage((string msg, IFile? file) ans)
        {
            if (ans.file == null)
            {
                return FailPage(ans.msg);
            }

            IMimeTypeService mimeTypeService = Resolve<IMimeTypeService>();
            string mimeType = mimeTypeService.GetMimeType(ans.file.DownloadPath);
            byte[] bytes = System.IO.File.ReadAllBytes(ans.file.PhysicPath);
            FileContentResult fileResult = new FileContentResult(bytes, mimeType);
            fileResult.FileDownloadName = ans.msg;
            return fileResult;
        }

        #region 日志相关函数
        protected void LogException(Exception ex, string module) => Client.LogException(ex, module, ControllerContext.ActionDescriptor.ActionName);
        protected void Log(string module, string relateId, string desc) => Client.Log(module, ControllerContext.ActionDescriptor.ActionName, relateId, desc);
        protected void Log(string module, Guid relateId, string desc) => Client.Log(module, ControllerContext.ActionDescriptor.ActionName, relateId.ToString(), desc);
        protected void Log(string module, EntityBase? entity, string desc) => Client.Log(module, ControllerContext.ActionDescriptor.ActionName, (entity?.ID ?? Guid.Empty).ToString(), desc);
        #endregion
    }
}
