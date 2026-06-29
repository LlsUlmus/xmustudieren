namespace Ricebird.Framework.Clients
{
    public interface IClientProvider : IScopedDependency
    {
        IClient CreateClient(HttpContext ctx);

        IClient CreateClient(IServiceScope scope, Guid key, string name);
    }
}
