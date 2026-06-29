global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Database;
global using System.ComponentModel.DataAnnotations.Schema;
global using static Ricebird.Framework.Utils;
using Ricebird.ShortUrls.Midwares;

namespace Ricebird.ShortUrls
{
    internal class ShortUrlModule : WebModule
    {
        public override string Name => "Ricebird.ShortUrls";

        public override int Priority => 17;

        public override string DisplayName => "短链接服务";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {
            app.UseMiddleware<ShortUrlMidware>();
        }

        public const string MODULE_NAME = "短链接服务";
    }
}
