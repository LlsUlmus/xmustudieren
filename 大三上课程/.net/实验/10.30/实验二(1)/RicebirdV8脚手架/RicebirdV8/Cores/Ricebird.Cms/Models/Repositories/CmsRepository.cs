using Ricebird.Cms.DataSources;
using Ricebird.Framework.DataValidator;

namespace Ricebird.Cms.Models
{
    public class CmsRepository(RicebirdContext ctx, IServiceProvider scoped, IContentChangeSource ccs) : RepositoryWithAttachment<Article, CmsAttachment>(ctx, scoped)
    {
        private readonly IContentChangeSource changeSource = ccs;

        /// <summary>
        /// 修改新闻
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public (ValidateResult validateResult, DbOperate opera, Article article) SaveArticle(IClient client)
        {
            var (opera, entity) = FillEntity(client, "CreatedBy", "CreatedOn");

            var result = entity.Validate(client);
            if (!result)
            {
                return (result, DbOperate.None, new Article());
            }

            // entity.VerifyStatus = VerifyStatus.Pass;
            entity.LastModified = DateTime.Now;
            entity.UpdatedOn = DateTime.Now;
            UpdateAttachments("附件", entity);
            SaveChanges();
            client.Log(MODULE_NAME, "SaveArticle", entity, $"保存了文章《{entity.Topic}》");

            changeSource.OnCategoryArticleChanged(client, entity.CategoryId);
            changeSource.OnArticleChanged(client, entity.ID);

            return (result, opera, entity);
        }

        /// <summary>
        /// 批量发布新闻
        /// </summary>
        /// <param name="client"></param>
        /// <param name="to"></param>
        /// <param name="artIds"></param>
        public void Publish(IClient client, Guid to, List<Guid> artIds)
        {
            DbSet.Where(e => artIds.Contains(e.ID)).ExecuteUpdate(set =>
                set.SetProperty(x => x.VerifyStatus, v => VerifyStatus.Pass)
            );
            SaveChanges();
            string log = $"发布了id为“{artIds.JoinAsString("，")}”的新闻";
            client.Log(MODULE_NAME, "Publish", to, log);

            List<Guid> ids = [.. DbSet.Where(e => artIds.Contains(e.ID)).Select(e => e.ID)];

            changeSource.OnCategoryArticleChanged(client, to);
            changeSource.OnArticleChanged(client, [.. ids]);
        }

        /// <summary>
        /// 批量撤稿
        /// </summary>
        /// <param name="to"></param>
        /// <param name="artIds"></param>
        public void Withdrawal(IClient client, Guid to, List<Guid> artIds)
        {
            DbSet.Where(e => artIds.Contains(e.ID)).ExecuteUpdate(set =>
                set.SetProperty(x => x.VerifyStatus, v => VerifyStatus.Deny)
            );
            SaveChanges();
            string log = $"撤销了id为“{artIds.JoinAsString("，")}”的新闻";
            client.Log(MODULE_NAME, "Withdrawal", to, log);

            List<Guid> ids = [.. DbSet.Where(e => artIds.Contains(e.ID)).Select(e => e.ID)];

            changeSource.OnCategoryArticleChanged(client, to);
            changeSource.OnArticleChanged(client, [.. ids]);
        }

        /// <summary>
        /// 批量剪切新闻
        /// </summary>
        /// <param name="to"></param>
        /// <param name="artIds"></param>
        /// <param name="toCate"></param>
        public void Cut(IClient client, Guid srcCategoryId, Guid to, List<Guid> artIds, Category toCate)
        {
            DbSet.Where(e => artIds.Contains(e.ID)).ExecuteUpdate(set =>
                set.SetProperty(x => x.CategoryId, v => to)
            );
            SaveChanges();
            string log = $"将id为“{artIds.JoinAsString("，")}”的新闻剪切至“{toCate}”栏目下";
            client.Log(MODULE_NAME, "Cut", to, log);

            changeSource.OnCategoryArticleChanged(client, srcCategoryId, to);
        }

        /// <summary>
        /// 批量复制新闻
        /// </summary>
        /// <param name="to"></param>
        /// <param name="artIds"></param>
        /// <param name="toCate"></param>
        public void Copy(IClient client, Guid to, List<Guid> artIds, Category toCate)
        {
            using var tranc = BeginTransaction();
            var articles = DbSet.AsNoTracking().Where(e => artIds.Contains(e.ID)).ToList();
            foreach (var art in articles)
            {
                art.BeNewEntity(client.CurrentUser, toCate);
                DbSet.Add(art);
            }

            var attachs = Attachments.AsNoTracking().Where(e => artIds.Contains(e.ArticleId)).ToList();
            foreach (var attach in attachs)
            {
                CmsAttachment newEntity = new CmsAttachment()
                {
                    ID = SequentialGuid.NewSuid(),
                    ArticleId = attach.ID,
                    FileId = attach.FileId,
                    Usage = attach.Usage,
                };
                Attachments.Add(newEntity);
            }
            SaveChanges();
            tranc.Commit();

            string log = $"将id为“{artIds.JoinAsString("，")}”的新闻复制至“{toCate}”栏目下";
            client.Log(MODULE_NAME, "Copy", to, log);

            changeSource.OnCategoryArticleChanged(client, to);
        }

        /// <summary>
        /// 批量删除新闻
        /// </summary>
        /// <param name="to"></param>
        /// <param name="artIds"></param>
        public void Delete(IClient client, Guid to, List<Guid> artIds)
        {
            List<Guid> ids = [.. DbSet.Where(e => artIds.Contains(e.ID)).Select(e => e.ID)];
            DbSet.Where(e => artIds.Contains(e.ID)).ExecuteDelete();
            SaveChanges();
            string log = $"删除了id为“{artIds.JoinAsString("，")}”的新闻";
            client.Log(MODULE_NAME, "Delete", to, log);

            changeSource.OnCategoryArticleChanged(client, to);
            changeSource.OnArticleChanged(client, [.. ids]);
        }

        /// <summary>
        /// 获得已经审核之后的新闻
        /// </summary>
        /// <param name="uniqueCode"></param>
        /// <returns></returns>
        public Article? GetValidArticle(string uniqueCode)
        {
            if (uniqueCode.TryParseToGuid(out Guid id))
            {
                return DbSet.FirstOrDefault(e => e.VerifyStatus == VerifyStatus.Pass && e.ID == id);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 更新点击次数
        /// </summary>
        /// <param name="article"></param>
        public void UpdateHit(Guid id)
        {
            DbSet
                .Where(e => e.ID == id)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.Hits, v => v.Hits + 1));
        }

        public override IQueryable<CmsAttachment> GetAttachmentsByForeignKey(Guid id) => Attachments.Where(e => e.ArticleId == id);
    }
}
