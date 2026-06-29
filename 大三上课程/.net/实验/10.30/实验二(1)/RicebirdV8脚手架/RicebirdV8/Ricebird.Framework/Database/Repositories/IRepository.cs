using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ricebird.Framework.Database
{
    public interface IRepository : IDependency, IDisposable
    {

    }

    public interface IRepository<TEntity> : IRepository
        where TEntity : EntityBase
    {
        #region 数据集
        public DbSet<TEntity> DbSet { get; }
        #endregion

        #region 基本的增删查改（同步函数）
        TEntity Insert(TEntity entity, bool autoSave = true);

        TEntity Update(TEntity entity, bool autoSave = false);

        TEntity? Get(Guid id);

        void Remove(TEntity entity, bool autoSave = false);

        void RemoveWhere(Expression<Func<TEntity, bool>> predicate, bool autoSave = false);
        #endregion

        #region IServiceProvider
        T Resolve<T>() where T : class;
        DbOperate Save(TEntity entity);
        TEntity FirstOrNew(Expression<Func<TEntity, bool>> predicate);
        #endregion
    }
}
