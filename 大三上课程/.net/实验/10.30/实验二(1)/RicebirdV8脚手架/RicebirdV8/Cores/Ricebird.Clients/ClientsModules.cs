global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Database;
using Ricebird.Clients.Midwares;

namespace Ricebird.Clients
{
    public class ClientsModules : WebModule
    {
        public override string Name => "Ricebird.Clients";

        public override int Priority => 50;

        public override string DisplayName => "客户端管理模块";

        public override void Register(IServiceCollection services)
        {
        }

        public override void Use(WebApplication app) => app.UseMiddleware<ClientMidware>();

        public const string MODULE_NAME = "客户端管理服务";
    }
}
