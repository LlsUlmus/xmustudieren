using Microsoft.Extensions.Primitives;

namespace Ricebird.Cms.DataSources
{
    public class PageBuilder(IClient client) : IScopedDependency
    {
        public Dictionary<string, PageDataSource> DataSources { get; set; } = [];

        public T ResolveDataSource<T>(string sourceName)
            where T : PageDataSource
        {
            if (DataSources.TryGetValue(sourceName, out var value))
            {
                if (value is T source)
                    return source;
                else
                    throw new InvalidCastException($"数据源是{value.GetType().Name}，但要求的是{typeof(T).Name}类型。");
            }

            var dataSource = client.Resolve<T>();
            DataSources.Add(sourceName, dataSource);

            return dataSource;
        }

        public ArticleDataSource ResolveArticleData<T>(string sourceName, string uniqueCode, int pageSize = 10, int page = 1)
            where T : ArticleDataSource
        {
            T dataSource = ResolveDataSource<T>(sourceName);
            dataSource.UniqueCode = uniqueCode;
            dataSource.PageInfo = (page, pageSize);
            var data = dataSource.GetData();
            return dataSource;
        }

        public ArticleDataSource ResolveArticleData(string sourceName, string uniqueCode, int pageSize = 10, int page = 1)
            => ResolveArticleData<ArticleDataSource>(sourceName, uniqueCode, pageSize, page);

        public ArticleDataSource ResolveImageArticleData(string sourceName, string uniqueCode, int pageSize = 10, int page = 1)
        {
            ArticleDataSource dataSource = ResolveDataSource<ArticleDataSource>(sourceName);
            dataSource.UniqueCode = uniqueCode;
            dataSource.HasFeatureImage = true;
            dataSource.PageInfo = (page, pageSize);
            var data = dataSource.GetData();
            return dataSource;
        }

        public IChangeToken BuildChangeToken()
        {
            var tokens = DataSources.Select(e => e.Value.ChangeToken).ToList();
            return new CompositeChangeToken(tokens);
        }
    }
}
