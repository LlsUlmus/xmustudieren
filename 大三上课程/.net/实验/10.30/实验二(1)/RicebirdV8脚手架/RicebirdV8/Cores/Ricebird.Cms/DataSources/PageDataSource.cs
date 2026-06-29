using Microsoft.Extensions.Primitives;

namespace Ricebird.Cms.DataSources
{
    public abstract class PageDataSource : IDependency
    {
        public IChangeToken ChangeToken { get; protected set; } = ContentChangeSource.EmptyToken;
        public string Memo { get; protected set; } = string.Empty;
    }

    public abstract class PageDataSource<T> : PageDataSource
        where T : class
    {
        public List<T> Result { get; protected set; } = [];
        public abstract List<T> GetData();
    }
}
