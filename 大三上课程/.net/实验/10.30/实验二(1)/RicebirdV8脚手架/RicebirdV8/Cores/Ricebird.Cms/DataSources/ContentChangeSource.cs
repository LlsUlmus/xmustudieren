using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace Ricebird.Cms.DataSources
{
    /// <summary>
    /// 内容变化控制器，未来考虑改成MemoryCache来管理CancellationTokenSource
    /// <para>
    /// 不过考虑应用场景里的新闻和分类都不多，哪怕是几万条都加载进来也就这么回事。所以就不改了
    /// </para>
    /// </summary>
    internal class ContentChangeSource : IContentChangeSource
    {
        private ConcurrentDictionary<Guid, CancellationTokenSource> ArticleTokenSource = [];
        private ConcurrentDictionary<Guid, CancellationTokenSource> ArticleInCategoryTokenSource = [];
        private ConcurrentDictionary<Guid, CancellationTokenSource> CategoryTokenSource = [];

        internal static IChangeToken EmptyToken
        {
            get; private set;
        }

        static ContentChangeSource()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var changeToken = new CancellationChangeToken(tokenSource.Token);
            tokenSource.Cancel();
            EmptyToken = changeToken;
        }

        public void OnArticleChanged(IClient client, params Guid[] articles)
        {
            foreach (var id in articles)
            {
                if (ArticleTokenSource.TryRemove(id, out var token))
                {
                    token.Cancel();
                }
            }
        }

        public void OnCategoryArticleChanged(IClient client, params Guid[] categories)
        {
            foreach (var id in categories)
            {
                if (ArticleInCategoryTokenSource.TryRemove(id, out var token))
                {
                    token.Cancel();
                }
            }
        }

        public void OnCategoryChanged(IClient client, params Guid[] categories)
        {
            Guid[] final = [Guid.Empty, .. categories];

            foreach (var id in final)
            {
                if (CategoryTokenSource.TryRemove(id, out var token))
                {
                    token.Cancel();
                }
            }
        }

        public IChangeToken BuildChangeToken(IEnumerable<Guid> articles, IEnumerable<Guid> articleInCategories, IEnumerable<Guid> categories)
        {
            if (articles.Count() + articleInCategories.Count() + categories.Count() == 0)
            {
                return EmptyToken;
            }

            List<IChangeToken> changeTokens = [];
            foreach (var id in articles)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                ArticleTokenSource.AddOrUpdate(id, id => tokenSource, (id, oldTokenSource) =>
                {
                    tokenSource = oldTokenSource;
                    return oldTokenSource;
                });

                changeTokens.Add(new CancellationChangeToken(tokenSource.Token));
            }

            foreach (var id in articleInCategories)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                ArticleInCategoryTokenSource.AddOrUpdate(id, id => tokenSource, (id, oldTokenSource) =>
                {
                    tokenSource = oldTokenSource;
                    return oldTokenSource;
                });

                changeTokens.Add(new CancellationChangeToken(tokenSource.Token));
            }

            foreach (var id in categories)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CategoryTokenSource.AddOrUpdate(id, id => tokenSource, (id, oldTokenSource) =>
                {
                    tokenSource = oldTokenSource;
                    return oldTokenSource;
                });

                changeTokens.Add(new CancellationChangeToken(tokenSource.Token));
            }

            return new CompositeChangeToken(changeTokens);
        }
    }
}
