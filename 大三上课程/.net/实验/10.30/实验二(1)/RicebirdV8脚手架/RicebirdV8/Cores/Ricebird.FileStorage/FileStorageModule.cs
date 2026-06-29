global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.FileStorage.Models;
global using Ricebird.FileStorage.Services;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Configurations;
global using Ricebird.Framework.Database;
global using Ricebird.Framework.FileStorage;
global using static Ricebird.Framework.Utils;

namespace Ricebird.FileStorage
{
    public class FileStorageModule : WebModule
    {
        public override string Name => "Ricebird.FileStorage";

        public override int Priority => 90;

        public override string DisplayName => "文件服务模块";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {
            using var scoped = app.Services.CreateScope();
            var service = (scoped.Resolve<IFileStorageService>() as FileStorageService)!;
            HostEnv.WriteLog(DisplayName, "正在清理所有临时文件");
            service.ClearTemporaryDir();
        }
    }
}
