using Ricebird.Clients.Services;
using Ricebird.Framework.AspNetCoreExtensions;
using Ricebird.Framework.Diagnostics.Features;

namespace Ricebird.Clients.Midwares
{
    internal class ClientMidware(RequestDelegate next) : RicebirdMidware(next)
    {
        public override async Task Invoke(HttpContext context)
        {
            //var cp = context.RequestServices.Resolve<IClientProvider>();
            //IClient client = cp.CreateClient(context);            
            IClient client = context.RequestServices.Resolve<IClient>();
            if (client is DefaultClient defClient)
            {
                defClient.BuildClient(context);
            }

            context.Features.Set<IClient>(client);

            var browser = context.Features.Get<Browser>();
            if (browser != null) client.Features.Set(browser);

            await _next(context);

            (client as DefaultClient)?.Dispose();
        }
    }
}
