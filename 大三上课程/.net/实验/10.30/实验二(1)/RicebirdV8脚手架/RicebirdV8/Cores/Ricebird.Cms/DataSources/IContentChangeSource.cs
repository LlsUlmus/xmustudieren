
using Microsoft.Extensions.Primitives;

namespace Ricebird.Cms.DataSources
{
    public interface IContentChangeSource : ISingletonDependency
    {
        void OnCategoryArticleChanged(IClient client, params Guid[] categories);
        void OnArticleChanged(IClient client, params Guid[] articles);
        void OnCategoryChanged(IClient client, params Guid[] categories);
        IChangeToken BuildChangeToken(IEnumerable<Guid> articles, IEnumerable<Guid> articleInCategories, IEnumerable<Guid> categories);
    }
}
