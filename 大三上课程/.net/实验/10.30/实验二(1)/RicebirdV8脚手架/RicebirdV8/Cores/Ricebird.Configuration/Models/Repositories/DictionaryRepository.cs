
namespace Ricebird.Configuration.Models
{
    internal class DictionaryRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<DataDictionary>(ctx, scoped)
    {
        public DbSet<DictionaryEntry> DictionaryEntries => DbContext.Set<DictionaryEntry>();
    }

    internal class EntryRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<DictionaryEntry>(ctx, scoped) { }
}
