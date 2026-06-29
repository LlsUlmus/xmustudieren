namespace Ricebird.Framework.Database.Searcher
{
    public abstract class AbstractSearcher<TEntity> : IScopedDependency
    {
        #region 字段
        public Guid ID
        {
            get; set;
        } = Guid.Empty;
        #endregion

        public abstract IQueryable<TEntity> BuildQuery();
    }
}
