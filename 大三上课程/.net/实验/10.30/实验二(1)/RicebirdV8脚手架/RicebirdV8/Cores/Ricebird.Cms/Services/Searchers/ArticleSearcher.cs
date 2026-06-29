using Ricebird.Cms.Models;
using Ricebird.Framework.Database.Searcher;

namespace Ricebird.Cms.Services.Searchers
{
    public class ArticleSearcher(IServiceProvider provider, CategoryService cateService) : AbstractPaginationSearcher<Article>
    {
        #region 可用的搜索内容
        public DateTime From
        {
            get; set;
        } = ConstKeys.MinDate;

        public DateTime To
        {
            get; set;
        } = ConstKeys.MaxDate;

        public string Topic
        {
            get; set;
        } = string.Empty;

        public Guid CategoryId
        {
            get; set;
        } = Guid.Empty;

        public VerifyStatus VerifyStatus
        {
            get; set;
        } = VerifyStatus.All;

        public Guid RelateId
        {
            get; set;
        } = Guid.Empty;

        /// <summary>
        /// -1 视为所有
        /// </summary>
        public int TopMost
        {
            get; set;
        } = -1;

        public bool HasContent
        {
            get; set;
        } = false;
        #endregion

        public override IQueryable<Article> BuildQuery()
        {
            var repo = provider.Resolve<CmsRepository>();
            var categoryService = provider.Resolve<CategoryService>();

            IQueryable<Article> query;
            if (HasContent)
            {
                query = from e in repo.DbSet
                        select new Article()
                        {
                            ID = e.ID,
                            CategoryId = e.CategoryId,
                            Author = e.Author,
                            VerifyStatus = e.VerifyStatus,
                            ReleaseTime = e.ReleaseTime,
                            Abstract = e.Abstract,
                            DisplayOrder = e.DisplayOrder,
                            Topic = e.Topic,
                            TopMost = e.TopMost,
                            SubTopic = e.SubTopic,
                            GuidOrder = e.GuidOrder,
                            IsOutLink = e.IsOutLink,
                            OutLink = e.OutLink,
                            FeaturedImage = e.FeaturedImage,
                            Content = e.Content,
                            DepartId = e.DepartId,
                            RelateId = e.RelateId,
                        };
            }
            else
            {
                query = from e in repo.DbSet
                        select new Article()
                        {
                            ID = e.ID,
                            CategoryId = e.CategoryId,
                            Author = e.Author,
                            VerifyStatus = e.VerifyStatus,
                            ReleaseTime = e.ReleaseTime,
                            Abstract = e.Abstract,
                            DisplayOrder = e.DisplayOrder,
                            Topic = e.Topic,
                            TopMost = e.TopMost,
                            SubTopic = e.SubTopic,
                            GuidOrder = e.GuidOrder,
                            IsOutLink = e.IsOutLink,
                            OutLink = e.OutLink,
                            FeaturedImage = e.FeaturedImage,
                            DepartId = e.DepartId,
                            RelateId = e.RelateId,
                        };
            }

            query = query.WhereIf(ID, e => e.ID == ID);
            query = query.WhereIf(Topic, e => e.Topic.Contains(Topic));
            query = query.Where(e => e.CategoryId == CategoryId);
            query = query.WhereIf(RelateId, e => e.RelateId == RelateId);

            if (VerifyStatus != VerifyStatus.All)
            {
                query = query.Where(e => e.VerifyStatus == VerifyStatus);
            }

            if (TopMost != -1 && Enum.TryParse(TopMost.ToString(), out TopMostType topMostType))
            {
                query = query.Where(e => e.TopMost == topMostType);
            }

            query = query.WhereIf(From, e => e.ReleaseTime >= From);
            query = query.WhereIf(To, e => e.ReleaseTime <= To);

            Category? cate = categoryService.GetCategory(CategoryId);
            if (cate != null)
            {
                query = cate.CategoryType switch
                {
                    CategoryType.Timeline => query.OrderBy(e => e.DisplayOrder),
                    _ => query.OrderByDescending(e => e.GuidOrder),
                };
            }
            else
            {
                query = query.OrderByDescending(e => e.GuidOrder);
            }

            return query;
        }

        public override Article PostProcessing(Article data)
        {
            data.CategoryName = cateService.GetCategory(data.CategoryId)?.Name ?? "未分类";

            return data;
        }
    }
}
