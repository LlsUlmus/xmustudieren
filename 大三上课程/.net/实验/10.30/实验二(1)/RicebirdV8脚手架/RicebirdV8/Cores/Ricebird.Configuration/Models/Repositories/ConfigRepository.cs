
namespace Ricebird.Configuration.Models
{
    internal class ConfigRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<Configuration>(ctx, scoped)
    {
        public DbSet<Configuration> Configurations => DbContext.Set<Configuration>();
    }
}
