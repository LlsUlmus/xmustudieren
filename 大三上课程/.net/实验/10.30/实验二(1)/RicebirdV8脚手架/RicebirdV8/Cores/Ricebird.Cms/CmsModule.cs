global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Cms.Services;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Database;
global using Ricebird.Framework.Security;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using static Ricebird.Cms.CmsModule;
global using static Ricebird.Framework.Utils;
using Ricebird.Cms.DataSources;

namespace Ricebird.Cms
{
    public class CmsModule : WebModule
    {
        public override string Name => "Ricebird.Cms";

        public override int Priority => 130;

        public override string DisplayName => "内容管理服务";

        public override void Register(IServiceCollection services)
        {
            services.AddOutputCache(opt =>
            {
                opt.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(3600)));
                opt.AddPolicy("Default", builder => builder.AddPolicy<PageCachePolicy>());
            });
        }

        public override void Use(WebApplication app)
        {

        }

        public const string MODULE_NAME = "Cms";
        internal const string INERNAL_CATEGORY_SOURCE = "INERNAL_CATEGORY_SOURCE";
    }
}
