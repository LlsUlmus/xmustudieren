using Ricebird.Cms.Models;
using System.Collections;

namespace Ricebird.Cms.DataSources
{
    public class ArticleDataSource(CmsRepository repo, IContentChangeSource changeSource, CategoryService cateService) : PageDataSource<Article>, IEnumerable<Article>
    {
        #region 可用的搜索内容
        public int Page
        {
            get; set;
        } = 1;

        public int PageSize
        {
            get; set;
        } = 10;

        public (int page, int pageSize) PageInfo
        {
            set => (Page, PageSize) = value;
        }

        public Guid ID
        {
            get; set;
        } = Guid.Empty;

        public string Topic
        {
            get; set;
        } = string.Empty;

        public Guid CategoryId
        {
            get; set;
        } = Guid.Empty;

        public string UniqueCode
        {
            get => CategoryId.To62String();
            set
            {
                CategoryId = SequentialGuid.From62String(value);
            }
        }

        public VerifyStatus VerifyStatus
        {
            get; set;
        } = VerifyStatus.All;

        public Guid RelateId
        {
            get; set;
        } = Guid.Empty;

        public bool HasContent
        {
            get; set;
        } = false;

        public bool HasAttachment
        {
            get; set;
        } = false;

        public bool HasFeatureImage
        {
            get; set;
        } = false;

        public bool IncludeAttachment
        {
            get; set;
        } = false;
        #endregion

        public int TotalRows
        {
            get;
            protected set;
        }

        public Category? Category
        {
            get;
            protected set;
        }

        public IQueryable<Article> BuildQuery()
        {
            IQueryable<Article> query;
            query = IncludeAttachment ? repo.DbSet.Include(e => e.Attachments).ThenInclude(e => e.File) : repo.DbSet;
            if (HasContent)
            {
                query = from e in query
                        where e.VerifyStatus == VerifyStatus.Pass
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
                            Attachments = e.Attachments,
                        };
            }
            else
            {
                query = from e in query
                        where e.VerifyStatus == VerifyStatus.Pass
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
                            Attachments = e.Attachments,
                        };
            }

            query = query.WhereIf(ID, e => e.ID == ID);
            query = query.WhereIf(Topic, e => e.Topic.Contains(Topic));
            query = query.WhereIf(RelateId, e => e.RelateId == RelateId);
            query = query.WhereIf(HasFeatureImage, e => e.FeaturedImage != "");

            Category? cate = cateService.GetCategory(CategoryId);
            if (cate != null)
            {
                Guid[] cateIds = [CategoryId, .. cate.AllChildren.Select(e => e.ID)];
                query = query.Where(e => cateIds.Contains(e.CategoryId));
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

            if (HasAttachment) query = query.Where(e => e.Attachments.Count > 0);

            TotalRows = query.Count();
            query = query.Skip((Page - 1) * PageSize).Take(PageSize);

            return query;
        }

        public override List<Article> GetData()
        {
            if (Result.Count > 0) return Result;
            Category = cateService.GetCategory(CategoryId) ?? cateService.Categories.FirstOrDefault(e => e.Name == "首页");

            var query = BuildQuery();
            Result = query.ToList();
            List<Guid> ids = [], categoryIds = [];
            Result.ForEach(e =>
            {
                ids.Add(e.ID);
                categoryIds.Add(e.CategoryId);
                var cate = cateService.GetCategory(e.CategoryId);
                e.CategoryName = cate?.Name ?? string.Empty;
            });

            ids = ids.Distinct().ToList();
            categoryIds = categoryIds.Distinct().ToList();
            ChangeToken = changeSource.BuildChangeToken(ids, categoryIds, []);
            Memo = $"""
                文章检索器 @ {DateTime.Now:H时m分s秒}
                文章源：{ids.Select(e => e.To62String()).JoinAsString("，")}
                分类源：{categoryIds.Select(e => e.To62String()).JoinAsString("，")}
                """;
            return Result;
        }

        public Article? GetArticle(Guid id)
        {
            ID = id;
            return GetData().FirstOrDefault();
        }

        public string GetCategoryLink(int page = 1) => $"/cms/{UniqueCode}-{page}";

        public IEnumerator<Article> GetEnumerator() => Result.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Result.GetEnumerator();
    }
}
