using Ricebird.Framework.FileStorage;
using Ricebird.Framework.Tools;
using Ricebird.Scedules.Services;

namespace Microsoft.AspNetCore.Builder
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// 这个必须放在UseRicebirdMidware后面
        /// </summary>
        /// <param name="app"></param>
        public static void UseScheduleStorage(this WebApplication app)
        {
            var mimeService = app.Services.Resolve<IMimeTypeService>();
            var provider = mimeService.BuildContentTypeProvider();

            // 永久文件处理器
            app.Use((ctx, next) =>
            {
                var pathString = ctx.Request.Path;
                IServiceProvider RequestServices = ctx.RequestServices;
                IOptionService os = RequestServices.Resolve<IOptionService>();
                var opt = os.LoadOptions<FileStorageOption>();
                ScheduleService sService = (RequestServices.Resolve<IScheduleService>() as ScheduleService)!;
                IMimeTypeService mimeTypeService = RequestServices.Resolve<IMimeTypeService>();

                // 这里处理的必须是永久保存的文件
                if (pathString.StartsWithSegments("/api/schedule/download", out PathString remaining))
                {
                    if (!Guid.TryParse(remaining.ToString()[1..], out Guid id))
                    {
                        return ResultPages.WarningPageAsync("找不到对应的资源", ctx);
                    }

                    RicebirdSchedule? schedule = sService.Schedules.FirstOrDefault(e => e.ID == id);
                    if (schedule is null)
                    {
                        return ResultPages.WarningPageAsync("找不到对应的任务", ctx);
                    }

                    if (schedule.Status == ScheduleStatus.Cancel)
                    {
                        return ResultPages.WarningPageAsync("该任务已取消", ctx);
                    }

                    if (schedule.Status != ScheduleStatus.Completed)
                    {
                        return ResultPages.InfoPageAsync("该任务还没有结束，请稍候", ctx);
                    }

                    if (schedule.File == null)
                    {
                        return ResultPages.InfoPageAsync("该任务没有对应的文件可以下载", ctx);
                    }

                    var path = schedule.File.PhysicPath;
                    var download = schedule.DownloadFileName;
                    try
                    {
                        string mimeType = mimeTypeService.GetMimeType(download);
                        using Stream stream = new FileStream(path, FileMode.Open);
                        byte[] buffer = stream.ReadAllBytes();
                        return ResultPages.ToFileAsync(buffer, mimeType, download, ctx, "");
                    }
                    catch (Exception ex)
                    {
                        ctx.Response.StatusCode = 500;

                        return ResultPages.ErrorPageAsync($"{path},{download}\n{ex.Message}\n{ex.StackTrace}", ctx);
                    }
                }

                return next();
            });
        }
    }
}
