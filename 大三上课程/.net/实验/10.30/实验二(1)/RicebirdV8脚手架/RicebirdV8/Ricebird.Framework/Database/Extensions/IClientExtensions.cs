using Ricebird.Framework.Database;

namespace Ricebird.Framework.Clients
{
    public static class IClientExtensions
    {
        public static DatabaseDiagnostic GetDbDiagnostic(this IClient client)
        {
            RicebirdContext ctx = client.Resolve<RicebirdContext>();
            return ctx.DbDiagnostic;
        }
    }
}
