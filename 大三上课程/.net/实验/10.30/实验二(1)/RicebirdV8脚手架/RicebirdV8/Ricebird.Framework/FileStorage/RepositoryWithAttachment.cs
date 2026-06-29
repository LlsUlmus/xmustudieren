#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.FileStorage;

namespace Ricebird.Framework.Database
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
{
    public abstract class RepositoryWithAttachment<TEntity, TAttachment>(RicebirdContext ctx, IServiceProvider scoped) : RepositoryWithAttachmentAndOtherEntity<TEntity, TEntity, TAttachment>(ctx, scoped)
        where TEntity : EntityWithAttachment<TAttachment>, new()
        where TAttachment : AttachmentEntityBase, new()
    {

    }

    public abstract class RepositoryWithAttachmentAndOtherEntity<TEntity, TOtherEntity, TAttachment>(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<TEntity>(ctx, scoped)
        where TEntity : EntityBase, new()
        where TOtherEntity : EntityWithAttachment<TAttachment>, new()
        where TAttachment : AttachmentEntityBase, new()
    {
        public DbSet<TAttachment> Attachments => DbContext.Set<TAttachment>();

        public virtual void UpdateAttachments(string usage, TOtherEntity data)
        {
            IFileStorageService fileStorage = Client.Resolve<IFileStorageService>();
            List<Guid> attachList = Client.GetList<Guid>(usage, "|");
            List<TAttachment> currentAttach = [];
            foreach (Guid id in attachList)
            {
                var file = fileStorage.GetFile(id);
                if (file is PermanentFile pf)
                {
                    var att = new TAttachment()
                    {
                        File = pf,
                        FileId = pf.ID,
                        Usage = usage,
                    };
                    att.SetForeingData(data);
                    currentAttach.Add(att);
                }
            }

            //using var tranc = BeginTransaction();
            GetAttachmentsByForeignKey(data.ID).Where(e => e.Usage == usage).ExecuteDelete();
            foreach (var item in currentAttach)
            {
                Attachments.Add(item);
            }

            SaveChanges();
            //tranc.Commit();
        }

        public abstract IQueryable<TAttachment> GetAttachmentsByForeignKey(Guid id);
    }
}
