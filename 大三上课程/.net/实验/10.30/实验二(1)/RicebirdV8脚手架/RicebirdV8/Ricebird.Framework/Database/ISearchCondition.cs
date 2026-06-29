namespace Ricebird.Framework.Database
{
    public interface ISearchCondition<TEntity>
        where TEntity : EntityBase, new()
    {
        IQueryable<TEntity> BuildQuery();
    }
}
