global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Configurations;
global using Ricebird.Framework.Database;
global using Ricebird.Framework.Security;
global using Ricebird.Security.Models;
global using Ricebird.Security.Models.Repositories;
global using Ricebird.Security.Services;
global using System.Collections.Frozen;
global using static Ricebird.Framework.Utils;
global using static Ricebird.Security.SecurityModule;
using Ricebird.Security.Midwares;

namespace Ricebird.Security
{
    public class SecurityModule : WebModule
    {
        public override string Name => "Ricebird.Security";

        public override int Priority => 70;

        public override string DisplayName => "安全管理模块";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {
            app.UseMiddleware<AuthenticationMidware>();
        }

        public const string MODULE_NAME = "安全管理模块";
    }
}
