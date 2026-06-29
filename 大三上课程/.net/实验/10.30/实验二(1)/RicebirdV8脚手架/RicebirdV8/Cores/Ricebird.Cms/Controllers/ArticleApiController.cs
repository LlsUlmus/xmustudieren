using Ricebird.Cms.Models;
using Ricebird.Cms.Services.Searchers;

namespace Ricebird.Cms.Controllers
{
    [Route("~/api/cms/articles/{action}"), ApiGroup("新闻管理")]
    public class ArticleApiController(CategoryService categoryService, CmsRepository artRepo) : ApiController
    {
        [ApiLinkTo("查看新闻列表", Permissions.EditArticle)]
        public ActionResult GetArticles()
        {
            ArticleSearcher searcher = Client.FillResolveObject<ArticleSearcher>();
            var (totalRow, page, pageSize, data) = searcher.BuildPaginationData();

            return Ok(new
            {
                success = true,
                msg = "",
                page,
                pageSize,
                totalRow,
                data
            });
        }

        [ApiLinkTo("查看新闻", Permissions.EditArticle)]
        public ActionResult GetArticleById()
        {
            Guid id = Get("id", Guid.Empty);

            var art = artRepo.DbSet.Include(e => e.Attachments).ThenInclude(e => e.File).FirstOrDefault(e => e.ID == id);
            if (art == null)
            {
                Guid categoryId = Get("categoryId", Guid.Empty);
                var cate = categoryService.GetCategory(categoryId);
                if (cate == null)
                {
                    return Fail($"新建时，找不到ID为{categoryId}的分类。");
                }

                art = new Article(CurrentUser, cate);
            }

            return Ok(new
            {
                success = true,
                msg = "",
                data = art
            }, "yyyy-MM-dd");
        }

        [ApiLinkTo("编辑新闻", Permissions.EditArticle)]
        public ActionResult SaveArticle()
        {
            var (result, _, entity) = artRepo.SaveArticle(Client);

            if (!result)
            {
                return ValidateError(result);
            }

            return Ok(new
            {
                success = true,
                msg = "",
                data = entity
            });
        }

        #region 新闻的批量发布，撤销，移动和删除
        [ApiLinkTo("批量编辑新闻", "编辑新闻")]
        public ActionResult MoveArticle()
        {
            Guid to = Get("to", Guid.Empty);
            string opera = Get("opera", "copy");
            List<Guid> value = GetList<Guid>("value");

            Category? cate = categoryService.Categories.FirstOrDefault(e => e.ID == to);
            if (cate == null)
            {
                return Ok(new
                {
                    success = false,
                    msg = "找不到操作所对应的栏目",
                    to
                });
            }

            if (value.Count == 0)
            {
                return Ok();
            }

            switch (opera)
            {
                case "publish":
                    return Publish(to, value);
                case "withdrawal":
                    return Withdrawal(to, value);
                case "cut":
                    Guid srcCategoryId = Get("srcCategoryId", Guid.Empty);
                    return Cut(srcCategoryId, to, value, cate);
                case "copy":
                    return Copy(to, value, cate);
                case "delete":
                    return Delete(to, value);
                default:
                    return Fail($"不支持名为{opera}的操作");
            }
        }

        private ActionResult Publish(Guid to, List<Guid> artIds)
        {
            artRepo.Publish(Client, to, artIds);
            return Ok();
        }

        private ActionResult Withdrawal(Guid to, List<Guid> artIds)
        {
            artRepo.Withdrawal(Client, to, artIds);
            return Ok();
        }

        private ActionResult Cut(Guid srcId, Guid to, List<Guid> artIds, Category toCate)
        {
            artRepo.Cut(Client, srcId, to, artIds, toCate);
            return Ok();
        }

        private ActionResult Copy(Guid to, List<Guid> artIds, Category toCate)
        {
            artRepo.Copy(Client, to, artIds, toCate);
            return Ok();
        }

        private ActionResult Delete(Guid to, List<Guid> artIds)
        {
            artRepo.Delete(Client, to, artIds);
            return Ok();
        }
        #endregion
    }
}
