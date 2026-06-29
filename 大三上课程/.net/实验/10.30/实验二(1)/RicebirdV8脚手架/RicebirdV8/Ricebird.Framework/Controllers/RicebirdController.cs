using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NPOI.SS.UserModel;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Controllers.RicebirdResults;
using Ricebird.Framework.Diagnostics.Features;
using Ricebird.Framework.FileStorage;
using Ricebird.Framework.Mvc.RicebirdResult;
using Ricebird.Framework.Scedules;
using Ricebird.Framework.Security;
using Ricebird.Framework.SignalR;
using Ricebird.Framework.Tools;
using SkiaSharp;

namespace Ricebird.Framework
{
    public abstract class RicebirdController : Controller
    {
        #region IClient
        public IClient Client
        {
            get
            {
                IClient? c = HttpContext.Features.Get<IClient>();
                return c ?? throw new DriveNotFoundException("找不到初始化IClient的服务，请确定在管道上存在程序集Ricebird.Clients");
            }
        }

        public Browser Browser => Client.Browser;

        public IUserPrincipal CurrentUser => Client.CurrentUser;

        public bool Successed(string permission) => Client.Successed(permission);

        public bool Successed(params string[] permissions) => Client.Successed(permissions);

        public string CurrentAction { get; private set; } = string.Empty;
        public string CurrentApi { get; private set; } = string.Empty;
        #endregion

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            OptionService = Client.Resolve<IOptionService>();
            ScheduleService = Client.Resolve<IScheduleService>();
            var filter = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(e => e.Filter.GetType().IsAssignableTo(typeof(Api)));
            CurrentAction = context.ActionDescriptor.DisplayName ?? "";
            if (filter?.Filter is Api apiFilter)
            {
                CurrentApi = apiFilter.Name;
            }
        }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public IOptionService OptionService
        {
            get; private set;
        }

        public IScheduleService ScheduleService
        {
            get; private set;
        }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public T Get<T>(string paramName, T defaultValue) => Client.Get(paramName, defaultValue);

        private static readonly char[] seperator = [','];
        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        protected virtual List<T> GetList<T>(string paramName) => GetList<T>(paramName, seperator);

        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        protected virtual List<T> GetList<T>(string paramName, char seperator) => GetList<T>(paramName, [seperator]);

        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        protected virtual List<T> GetList<T>(string paramName, string seperator) => GetList<T>(paramName, seperator.ToArray());

        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        protected virtual List<T> GetList<T>(string paramName, char[] seperator)
        {
            try
            {
                string txt = Get(paramName, "");

                List<T> result = txt.Split(seperator, StringSplitOptions.RemoveEmptyEntries).Select(e => ValueUtils.ChangeToType<T>(e)!).ToList();

                return result;
            }
            catch
            {
                return [];
            }

        }

        #region 取变量
        public T Resolve<T>()
            where T : class => Client.Resolve<T>();
        #endregion

        #region ActionResult
        protected virtual ActionResult RJson(object obj, string dateTimeFormat = "yyyy年M月d日")
        {
            RicebirdJsonResult jsonResult = new RicebirdJsonResult(obj, false, dateTimeFormat);
            return jsonResult;
        }

        protected virtual ActionResult JsonP(object obj)
        {
            string callback = Get("callback", "callback");
            string template = $"{callback}({obj.SearializeJson()})";

            return Content(template);
        }

        protected virtual ActionResult Image(SKBitmap bitmap, SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg, int quality = 100) => new RicebirdBitmapResult(bitmap, format, quality);

        protected virtual ExcelResult Excel(IWorkbook workbook)
        {
            return new ExcelResult(workbook, string.Empty);
        }

        protected virtual ExcelResult Excel(IWorkbook workbook, string workbookName)
        {
            return new ExcelResult(workbook, workbookName);
        }
        #endregion

        #region Schedule
        protected RicebirdSchedule CreateSchedule(string name)
        {
            return ScheduleService.CreateSchedule(Client, name);
        }

        protected ImportSchedule CreateImportSchedule(string name)
        {
            ISmsService sms = Client.Resolve<ISmsService>();
            IFileStorageService fileService = Client.Resolve<IFileStorageService>();
            ImportSchedule schedule = new ImportSchedule(sms, fileService, Client, name, HostEnv.Instance)
            {
                LinkToApi = CurrentApi
            };
            ScheduleService.CreateSchedule(schedule);
            return schedule;
        }

        protected ImportSchedule CreateImportSchedule(string name, string backupDir)
        {
            ISmsService sms = Client.Resolve<ISmsService>();
            IFileStorageService fileService = Client.Resolve<IFileStorageService>();
            ImportSchedule schedule = new ImportSchedule(sms, fileService, Client, name, HostEnv.Instance)
            {
                LinkToApi = CurrentApi,
                BackupDirectory = backupDir,
            };
            ScheduleService.CreateSchedule(schedule);
            return schedule;
        }

        protected ExportSchedule CreateExportSchedule(string name)
        {
            ISmsService sms = Client.Resolve<ISmsService>();
            IFileStorageService fileService = Client.Resolve<IFileStorageService>();
            ExportSchedule schedule = new ExportSchedule(sms, fileService, Client, name, HostEnv.Instance)
            {
                LinkToApi = CurrentApi
            };
            ScheduleService.CreateSchedule(schedule);
            return schedule;
        }

        protected ExportSchedule CreateExportSchedule(string name, string templatePath)
        {
            ISmsService sms = Client.Resolve<ISmsService>();
            IFileStorageService fileService = Client.Resolve<IFileStorageService>();
            ExportSchedule schedule = new ExportSchedule(sms, fileService, Client, name, HostEnv.Instance)
            {
                TemplatePath = templatePath,
                LinkToApi = CurrentApi
            };
            ScheduleService.CreateSchedule(schedule);
            return schedule;
        }
        #endregion

        #region 返回页面
        public virtual ActionResult SuccessPage(string message)
        {
            return ResultPages.ResultPage("success", message);
        }

        public virtual ActionResult WarningPage(string message)
        {
            return ResultPages.ResultPage("warning", message);
        }

        public virtual ActionResult InfoPage(string message)
        {
            return ResultPages.ResultPage("info", message);
        }

        public virtual ActionResult ErrorPage(string message)
        {
            return ResultPages.ResultPage("error", message);
        }
        #endregion
    }
}
