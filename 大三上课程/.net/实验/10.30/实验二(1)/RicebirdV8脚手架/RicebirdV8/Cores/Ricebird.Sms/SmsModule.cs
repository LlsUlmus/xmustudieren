global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.SignalR;
global using static Ricebird.Framework.Utils;
using Microsoft.AspNetCore.SignalR;
using Ricebird.Sms.Hubs;

namespace Ricebird.Sms
{
    internal class SmsModule : WebModule
    {
        public override string Name => "Ricebird.Sms";

        public override int Priority => 80;

        public override string DisplayName => "短消息模块";

        public override void Register(IServiceCollection services)
        {
            services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
        }

        public override void Use(WebApplication app)
        {
            app.MapHub<RicebirdHub>("/signalr/hubs/ricebird");
        }

        public const string MODULE_NAME = "短消息模块";
    }
}
