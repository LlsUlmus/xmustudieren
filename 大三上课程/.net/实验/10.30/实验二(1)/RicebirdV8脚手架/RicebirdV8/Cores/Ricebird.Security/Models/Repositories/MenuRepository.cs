namespace Ricebird.Security.Models.Repositories
{
    internal class MenuRepository(RicebirdContext ctx, IServiceProvider scoped) : TreeRepositoryBase<MenuItem>(ctx, scoped)
    {
    }
}
