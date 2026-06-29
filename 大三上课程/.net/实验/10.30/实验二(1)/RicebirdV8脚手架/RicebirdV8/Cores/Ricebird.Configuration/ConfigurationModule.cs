global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Configuration.Models;
global using Ricebird.Configuration.Services;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Configurations;
global using Ricebird.Framework.Database;
global using Ricebird.Framework.DataValidator;
global using Ricebird.Framework.Security;
global using static Ricebird.Configuration.ConfigurationModule;
global using static Ricebird.Framework.Utils;

namespace Ricebird.Configuration
{
    public class ConfigurationModule : WebModule
    {
        public override string Name => "Ricebird.Configuration";

        public override int Priority => 30;

        public override string DisplayName => "配置管理模块";

        public override void Register(IServiceCollection services)
        {

        }
        public override void Use(WebApplication app)
        {

        }

        public const string MODULE_NAME = "配置管理服务";
    }
}
