global using Ricebird.Diagnostics.Models;
global using Ricebird.Framework;
global using Ricebird.Framework.Diagnostics;
global using System;
global using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ricebird.Diagnostics.Services;

namespace Ricebird.Diagnostics
{
    internal class DiagnosticModule : WebModule
    {
        public override string Name => "Ricebird.Diagnostics";

        public override int Priority => 11;

        public override string DisplayName => "系统诊断模块";

        public override void Register(IServiceCollection services)
        {
            var opt = HostEnv.FrameworkOptions;

            if (opt.DiagnosticsDatabase != null)
            {
                services.AddDbContext<DiagnosticsContext>(options =>
                {
                    options.UseSqlServer(opt.DiagnosticsDatabase.ConnectionString, b => b.MigrationsAssembly(opt.WebAssemblyName));
                });

                services.AddScoped<IDbLogger, DefaultLogger>();
            }
            else
            {
                services.AddScoped<IDbLogger, EmptyDbLogger>();
            }
        }

        public override void Use(WebApplication app)
        {
            HostEnv env = HostEnv;

            if (env.ShouldLog())
            {
                using var scope = app.Services.CreateScope();
                var ctx = scope.ServiceProvider.Resolve<DiagnosticsContext>();

                var pendings = ctx.Database.GetPendingMigrations();
                if (pendings.Any())
                {
                    env.WriteLog("DataEngine", $"检测有{pendings.Count()}个日志数据表变更需要迁移，自动迁移至日志数据库！");
                    ctx.Database.Migrate();
                }
            }

            // app.UseMiddleware<DiagnosticMidware>();
        }
    }
}
