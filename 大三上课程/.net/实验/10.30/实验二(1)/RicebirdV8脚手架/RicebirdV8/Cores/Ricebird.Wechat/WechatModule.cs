global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Configurations;
global using Ricebird.Framework.Security;
global using Ricebird.Framework.SignalR;
global using static Ricebird.Framework.Utils;
global using static Ricebird.Wechat.WechatModule;

namespace Ricebird.Wechat
{
    internal class WechatModule : WebModule
    {
        public override string Name => "Ricebird.Wechat";

        public override int Priority => 17;

        public override string DisplayName => "微信模块";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {

        }

        public const string MODULE_NAME = "微信模块";
    }
}
