using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Security;
using System.Linq.Expressions;

namespace Ricebird.Framework.Database
{
    public class RepositoryBase<TEntity> : IRepository<TEntity>
        where TEntity : EntityBase, new()
    {
        public RepositoryBase(RicebirdContext ctx, IServiceProvider scoped)
        {
            DbContext = ctx;
            Provider = scoped;
            HostEnv = Provider.Resolve<HostEnv>();
            Client = scoped.Resolve<IClient>();
            OptionService = scoped.Resolve<IOptionService>();
        }

        public RicebirdContext DbContext
        {
            get; set;
        }

        public DatabaseFacade Database => DbContext.Database;

        protected IClient Client
        {
            get; set;
        }

        protected IUserPrincipal CurrentUser => Client.CurrentUser;

        protected HostEnv HostEnv { get; set; }

        public IOptionService OptionService
        {
            get; set;
        }

        public IServiceProvider Provider
        {
            get; set;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        #region SaveChanges
        public int SaveChanges()
        {
            return DbContext.SaveChanges();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return DbContext.SaveChanges(acceptAllChangesOnSuccess);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public IDbContextTransaction BeginTransaction() => Database.BeginTransaction();
        #endregion

        #region IQueryable
        public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

        protected StringBuilder CreateSqlBuilder() => (new StringBuilder()).AppendLine($"USE [{HostEnv.FrameworkOptions.Database.Catalog}]");
        #endregion

        #region 基本的增删查改（同步函数）
        public virtual TEntity CreateNew()
        {
            TEntity entity = new TEntity
            {
                EntityStatus = EntityStatus.New
            };

            if (entity is EntityWithTime et)
            {
                et.CreatedOn = DateTime.Now;
            }

            return entity;
        }

        public DbOperate Save(TEntity entity)
        {
            return Save(entity, true);
        }

        public DbOperate Save(TEntity entity, bool autoSave = true)
        {
            TEntity? exists = DbSet.FirstOrDefault(e => e.ID == entity.ID);
            bool editFlag = true;
            DbOperate ret = DbOperate.Update;
            if (exists == null)
            {
                editFlag = false;
                exists = new TEntity();
                DbSet.Add(exists);
                ret = DbOperate.Create;
            }
            else if (entity is EntityWithTime && editFlag)
            {
                DbContext.Entry(entity).Property("CreatedOn").IsModified = false;
            }

            exists.CopyPropertiesFrom(entity);

            if (autoSave)
            {
                SaveChanges();
            }

            return ret;
        }

        public DbOperate Save(object entity, bool autoSave = true)
        {
            return Save(entity, autoSave, string.Empty);
        }

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="autoSave"></param>
        /// <param name="ignoreProperties">如果是新实体，则会保存ignoreProperties</param>
        public virtual DbOperate Save(object entity, bool autoSave = true, params string[] ignoreProperties)
        {
            Guid id = entity.GetPropertyValue("ID", Guid.Empty);
            DbOperate ret = DbOperate.Update;
            if (id != Guid.Empty)
            {
                TEntity? exists = DbSet.FirstOrDefault(e => e.ID == id);
                if (exists == null)
                {
                    exists = new TEntity();
                    DbSet.Add(exists);
                    exists.CopyFromObject(entity);
                    ret = DbOperate.Create;
                }
                else
                {
                    exists.CopyFromObject(entity, ignoreProperties);
                }

                if (autoSave)
                {
                    SaveChanges();
                }
                return ret;
            }
            return DbOperate.None;
        }


        public virtual TEntity Insert(TEntity entity, bool autoSave = true)
        {
            var saved = DbSet.Add(entity).Entity;

            if (autoSave)
            {
                SaveChanges();
            }

            return saved;
        }

        public virtual TEntity Update(TEntity entity, bool autoSave = true)
        {
            SafeAttach(entity, true);

            if (entity is EntityWithTime et)
            {
                et.UpdatedOn = DateTime.Now;
                var entry = DbContext.Entry(entity);
                entry.Property("CreatedOn").IsModified = false;
            }

            var updated = DbSet.Update(entity).Entity;

            if (autoSave)
            {
                SaveChanges();
            }

            return updated;
        }

        public virtual TEntity Update(TEntity entity, bool autoSave = true, params string[] fields)
        {
            SafeAttach(entity);

            var entry = DbContext.Entry(entity);
            if (fields?.Length > 0)
            {
                foreach (var item in fields)
                {
                    try
                    {
                        var prop = entry.Property(item);
                        if (prop != null)
                        {
                            prop.IsModified = true;
                        }
                    }
                    catch (InvalidOperationException)
                    { }
                }
            }
            var updated = entry.Entity;

            if (autoSave)
            {
                SaveChanges();
            }

            return updated;
        }

        public virtual TEntity Update(TEntity entity, object fields, bool autoSave = true)
        {
            SafeAttach(entity);

            var entry = DbContext.Entry(entity);
            entry.CurrentValues.SetValues(fields);
            var updated = entry.Entity;

            if (autoSave)
            {
                SaveChanges();
            }

            return updated;
        }

        public virtual TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity? entity = DbSet.FirstOrDefault(predicate);
            if (entity != null)
            {
                entity.Client = Client;
            }

            return entity;
        }

        /// <summary>
        /// 从数据库中查找一个对象。如果对象不存在，则新建一个。
        /// <para>
        /// 通过这个方法查找出来的对象，可以直接调用Save函数
        /// </para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual TEntity FirstOrNew(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity? entity = DbSet.FirstOrDefault(predicate);
            if (entity == null)
            {
                entity = new TEntity()
                {
                    EntityStatus = EntityStatus.New,
                    Client = Client,
                };
            }
            else
            {
                entity.EntityStatus = EntityStatus.Exists;
                entity.Client = Client;
            }

            return entity;
        }

        public virtual (DbOperate operate, TEntity entity) FillDeserializeEntity(IClient client)
        {
            return FillDeserializeEntity<TEntity>(client, []);
        }

        public virtual (DbOperate operate, T entity) FillDeserializeEntity<T>(IClient client)
            where T : EntityBase, new()
        {
            return FillDeserializeEntity<T>(client, []);
        }

        public virtual (DbOperate operate, T entity) FillDeserializeEntity<T>(IClient client, params string[] ignoreProperties)
            where T : EntityBase, new()
        {
            T data = client.Deserialize<T>() ?? new T()
            {
                ID = Guid.Empty
            };
            DbSet<T> dbSet = DbContext.Set<T>();
            DbOperate dbOperate = DbOperate.Update;
            if (data.ID == Guid.Empty)
            {
                data.ID = SequentialGuid.NewSuid();
                dbSet.Add(data);
                dbOperate = DbOperate.Create;
            }
            else
            {
                T? exists = dbSet.FirstOrDefault(e => e.ID == data.ID);
                if (exists is null)
                {
                    data.ID = SequentialGuid.NewSuid();
                    dbSet.Add(data);
                    dbOperate = DbOperate.Create;
                }
                else
                {
                    exists.CopyFromObject(data, ignoreProperties);
                    data = exists;
                }
            }

            return (dbOperate, data);
        }

        public virtual (DbOperate operate, TEntity entity) FillEntity(IClient client)
        {
            return FillEntity(client, []);
        }
        public virtual (DbOperate operate, TEntity entity, TEntity old) FillEntityWithOld(IClient client, params string[] ignoreProperties)
        {
            TEntity old = new TEntity();
            TEntity data = new TEntity();
            client.FillObject(data, ignoreProperties);

            DbOperate dbOperate = DbOperate.Update;
            if (data.ID == Guid.Empty)
            {
                data.ID = SequentialGuid.NewSuid();
                RepositoryBase<TEntity>.RefillEmpty(data, ignoreProperties);
                DbSet.Add(data);
                dbOperate = DbOperate.Create;
            }
            else
            {
                TEntity? exists = FirstOrDefault(e => e.ID == data.ID);
                if (exists is null)
                {
                    data.ID = SequentialGuid.NewSuid();
                    RepositoryBase<TEntity>.RefillEmpty(data, ignoreProperties);
                    DbSet.Add(data);
                    dbOperate = DbOperate.Create;
                }
                else
                {
                    old.CopyFromObject(exists);
                    exists.CopyFromObject(data, ignoreProperties);
                    data = exists;
                }
            }

            return (dbOperate, data, old);
        }

        public virtual (DbOperate operate, TEntity entity) FillEntity(IClient client, params string[] ignoreProperties)
        {
            TEntity data = new TEntity();
            client.FillObject(data, ignoreProperties);

            DbOperate dbOperate = DbOperate.Update;
            if (data.ID == Guid.Empty)
            {
                data.ID = SequentialGuid.NewSuid();
                RepositoryBase<TEntity>.RefillEmpty(data, ignoreProperties);
                DbSet.Add(data);
                dbOperate = DbOperate.Create;
            }
            else
            {
                TEntity? exists = FirstOrDefault(e => e.ID == data.ID);
                if (exists is null)
                {
                    data.ID = SequentialGuid.NewSuid();
                    RepositoryBase<TEntity>.RefillEmpty(data, ignoreProperties);
                    DbSet.Add(data);
                    dbOperate = DbOperate.Create;
                }
                else
                {
                    exists.CopyFromObject(data, ignoreProperties);
                    data = exists;
                }
            }

            return (dbOperate, data);
        }

        private static void RefillEmpty(TEntity entity, params string[] refillFields)
        {
            if (refillFields.Length > 0)
            {
                TEntity empty = new TEntity();
                Type type = typeof(TEntity);
                foreach (var prop in refillFields)
                {
                    var info = type.GetProperty(prop);
                    if (info == null || !info.CanRead || !info.CanWrite) continue;

                    info.SetValue(entity, info.GetValue(empty), null);
                }
            }
        }

        //public virtual void Save(TEntity entity)
        //{
        //    _ = entity.Status switch
        //    {
        //        EntityStatus.New => Insert(entity),
        //        EntityStatus.Exists => Update(entity),
        //        _ => throw new InvalidOperationException("必须是通过储存器查询出来的方法，才可以直接使用本函数")
        //    };
        //}

        public virtual TEntity? Get(Guid id)
        {
            return DbSet.FirstOrDefault(e => e.ID == id);
        }

        public virtual void Remove(TEntity entity, bool autoSave = false)
        {
            SafeAttach(entity);
            DbSet.Remove(entity);
            if (autoSave)
            {
                SaveChanges();
            }
        }

        public virtual void RemoveWhere(Expression<Func<TEntity, bool>> predicate, bool autoSave = false)
        {
            DbSet.Where(predicate).ExecuteDelete();

            if (autoSave)
            {
                SaveChanges();
            }
        }
        #endregion

        #region Attach方法
        /// <summary>
        /// 安全的附加方法
        /// </summary>
        /// <param name="entity">待附加实体</param>
        /// <param name="setAllChangedFlag">附加实体后，设置实体的修改状态</param>
        protected void SafeAttach(TEntity entity, bool setAllChangedFlag = false)
        {
            if (!IsAttach(entity))
            {
                DbSet.Attach(entity);
            }

            if (setAllChangedFlag)
            {
                DbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        protected bool IsAttach(TEntity entity)
        {
            TEntity? local = DbSet.Local.FirstOrDefault(e => e.ID == entity.ID);
            if (local != null)
            {
                if (DbContext.Entry(entity).State != EntityState.Detached)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region IServiceProvider
        public virtual T Resolve<T>()
            where T : class
        {
            return Client.Resolve<T>();
        }
        #endregion
    }
}
