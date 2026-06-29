using Microsoft.EntityFrameworkCore;

namespace Ricebird.Framework.Database
{
    public static class IServiceCollectionExtension
    {
        public static void AddSqlServerDatabase(this IServiceCollection services, HostEnv env)
        {
            var FrameworkOptions = env.FrameworkOptions;
            if (string.IsNullOrWhiteSpace(FrameworkOptions.WebAssemblyName))
            {
#pragma warning disable CA2208 // 正确实例化参数异常
                throw new ArgumentNullException(nameof(FrameworkOptions.WebAssemblyName), $"在配置文件中，未配置WebAssemblyName项，此项用以表明哪个项目为启动项目。");
#pragma warning restore CA2208 // 正确实例化参数异常
            }

            string connStr = FrameworkOptions.Database.ConnectionString;
            services.AddDbContextPool<RicebirdContext>(options =>
            {
                options.UseSqlServer(connStr, b =>
                {
                    b.MigrationsAssembly(FrameworkOptions.WebAssemblyName);
                });
            }, 25);
        }

        public static void PendingMigration(this IApplicationBuilder app)
        {
            HostEnv env = app.ApplicationServices.Resolve<HostEnv>();
            using var scope = app.ApplicationServices.CreateScope();
            var ctx = scope.Resolve<RicebirdContext>();

            var pendings = ctx.Database.GetPendingMigrations();
            if (pendings.Any())
            {
                env.WriteLog("DataEngine", $"检测有{pendings.Count()}个业务数据表变更需要迁移，自动迁移至业务数据库！");
                ctx.Database.Migrate();
            }
        }
    }
}
