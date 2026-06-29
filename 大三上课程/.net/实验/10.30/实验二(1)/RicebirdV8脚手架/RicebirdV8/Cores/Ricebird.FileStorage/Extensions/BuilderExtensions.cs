using Ricebird.Framework.Tools;

namespace Microsoft.AspNetCore.Builder
{
    public static class BuilderExtensions
    {
        public static void UseFileStorage(this WebApplication app)
        {
            var mimeService = app.Services.Resolve<IMimeTypeService>();
            var provider = mimeService.BuildContentTypeProvider();

            // 永久文件处理器
            app.Use((ctx, next) =>
            {
                var pathString = ctx.Request.Path;
                IFileStorageService fileStorageService = ctx.RequestServices.GetRequiredService<IFileStorageService>();

                if (fileStorageService.IsFileInStorage(pathString))
                {
                    (byte[]? buffer, string mimeType, string downloadFileName, string displayName) = fileStorageService.GetFileBytes(pathString);
                    if (buffer == null)
                    {
                        return ResultPages.WarningPageAsync("找不到对应资源", ctx);
                    }

                    return ResultPages.ToFileAsync(buffer, mimeType, downloadFileName, ctx, displayName);
                }

                return next();
            });
        }
    }
}
