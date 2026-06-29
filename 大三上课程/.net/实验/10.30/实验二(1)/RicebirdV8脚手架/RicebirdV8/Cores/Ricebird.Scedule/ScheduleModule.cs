global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Configurations;
global using Ricebird.Framework.Scedules;
global using Ricebird.Framework.Security;
global using Ricebird.Framework.SignalR;
global using static Ricebird.Framework.Utils;

namespace Ricebird.Scedules
{
    internal class ScheduleModule : WebModule
    {
        public override string Name => "Ricebird.Scedules";

        public override int Priority => 90;

        public override string DisplayName => "任务管理模块";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {

        }

        public const string MODULE_NAME = "任务管理模块";
    }
}
