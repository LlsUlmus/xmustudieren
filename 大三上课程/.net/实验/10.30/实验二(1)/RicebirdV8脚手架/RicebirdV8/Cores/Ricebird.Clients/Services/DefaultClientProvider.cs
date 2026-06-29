namespace Ricebird.Clients.Services
{
    public class DefaultClientProvider(IServiceProvider services) : IClientProvider
    {
        private IServiceProvider Services { get; set; } = services;

        public IClient CreateClient(HttpContext ctx)
        {
            DefaultClient client = ctx.RequestServices.Resolve<DefaultClient>();
            client.BuildClient(ctx);
            return client;
        }

        public IClient CreateClient(IServiceScope scope, Guid key, string name)
        {
            DefaultClient client = Services.Resolve<DefaultClient>();
            client.BuildClient(scope, key, name);
            return client;
        }
    }
}
