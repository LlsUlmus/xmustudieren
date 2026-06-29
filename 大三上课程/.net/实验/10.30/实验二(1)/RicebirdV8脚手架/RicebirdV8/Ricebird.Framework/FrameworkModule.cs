global using Microsoft.Extensions.Caching.Memory;
global using Ricebird.Framework;
global using Ricebird.Framework.Configurations;
global using System;
global using System.Collections.Frozen;
global using System.ComponentModel.DataAnnotations;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using static Ricebird.Framework.Utils;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Ricebird.Framework.AspNetCoreExtensions.Binder;
using Ricebird.Framework.AspNetCoreExtensions.Filters;
using Ricebird.Framework.AspNetCoreExtensions.RateLimiters;
using Ricebird.Framework.Database;
using Ricebird.Framework.Diagnostics.Features;

namespace Ricebird.Framework
{
    public class FrameworkModule : WebModule
    {
        public override string Name => "Ricebird.Framework";

        public override int Priority => 10;

        public override string DisplayName => "米雀框架V8基本服务";

        public override void Register(IServiceCollection services)
        {
            services.AddTransient<IWorkbook, XSSFWorkbook>();
            services.AddKeyedTransient<IWorkbook, XSSFWorkbook>("xlsx");
            services.AddKeyedTransient<IWorkbook, HSSFWorkbook>("xls");

            services.AddAntiforgery(opt =>
            {
                opt.HeaderName = "X-CSRF-TOKEN";
            });

            if (HostEnv.IsDevelopment())
            {
                services.AddCors(opt =>
                {
                    opt.AddPolicy(ConstKeys.CorsAny, def =>
                    {
                        def.AllowAnyHeader();
                        def.AllowAnyMethod();
                        def.AllowAnyOrigin();
                    });
                });
            }

            services.AddControllersWithViews(opt =>
            {
                opt.Filters.Add<ExceptionFilter>();
                opt.ModelBinderProviders.Insert(0, new RicebirdEntityBinderProvider());
            });

            services.AddHttpContextAccessor();

            services.AddSqlServerDatabase(HostEnv);

            services.AddRateLimiter(ro =>
            {
                ro.AddPolicy<string, LimitByUser>(RateLimitPolicyKeys.按用户限流);
            });

            services.AddMvc();
        }

        public override void Use(WebApplication app)
        {
            IWebHostEnvironment env = app.Services.Resolve<IWebHostEnvironment>();
            HostEnv.AppRootPath = env.ContentRootPath;
            HostEnv.WebRootPath = env.WebRootPath;

            HostEnv.ServiceProvider = app.Services;
            ValueUtils.ServiceProvider = app.Services;
            HtmlChecker.HtmlChecker checker = app.Services.Resolve<HtmlChecker.HtmlChecker>();
            Utils.htmlChecker = checker;

            app.PendingMigration();

            app.Use(async (ctx, next) =>
            {
                ctx.Features.Set(new Browser(ctx));
                await next();
            });
        }
    }
}
