namespace Ricebird.FileStorage.Models
{
    public class PermanentFileRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<PermanentFile>(ctx, scoped)
    {
    }
}
