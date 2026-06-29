using Ricebird.Cms.Models;
using System.Text.Json;

namespace Ricebird.Cms.Controllers
{
    [Route("~/api/cms/category/{action}"), ApiGroup("新闻管理")]
    public class CategoryApiController(CategoryService cs) : ApiController
    {
        #region ctor
        public CategoryService categoryService = cs;
        #endregion

        [ApiLinkTo("获取栏目列表", "/manage/cms/categories")]
        public ActionResult GetCategoryTree()
        {
            Guid pid = Get("id", Guid.Empty);
            bool withEmpty = Get(nameof(withEmpty), true);
            bool force = Get(nameof(force), false);

            if (force || categoryService.Categories.Count == 0)
            {
                categoryService.LoadCategory();
            }

            var query = categoryService.Categories.Where(e => e.ParentId == pid)
                .Select(e => e.ConvertToJson());

            var data = new List<object>();
            if (withEmpty)
            {
                data.Add(new
                {
                    key = Guid.Empty,
                    id = Guid.Empty,
                    pid = Guid.Empty,
                    pn = "无",
                    display = "所有栏目",
                    title = "所有栏目",
                    name = "所有栏目",
                    type = 0
                });
            }

            data.AddRange(query);

            bool hasHome = categoryService.Categories.Any(e => e.CategoryType == CategoryType.Home);

            return Ok(new
            {
                success = true,
                msg = "",
                hasHome,
                data
            });
        }

        [ApiLinkTo("获取栏目", Permissions.EditArticle)]
        public ActionResult GetCategory()
        {
            var (entity, ar) = InternalGetCategory();
            entity ??= new Category()
            {
                ID = Guid.Empty
            };

            if (entity.ID == Guid.Empty)
            {
                entity.Children = categoryService.Categories.Where(e => e.ParentId == Guid.Empty).ToList();
            }

            var validator = entity.BuildValidator();

            return Ok(new
            {
                success = true,
                msg = "",
                data = entity,
                entity.Children,
                rules = validator.ToJsonObject()
            });
        }

        [ApiShouldAuthorize("编辑栏目")]
        public ActionResult SaveCategory()
        {
            CategoryRepository cr = Resolve<CategoryRepository>();

            var (result, _, entity) = cr.SaveCategory(Client);
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

        [ApiLinkTo("新建栏目", "编辑栏目")]
        public ActionResult CreateCategories()
        {
            CreateCategoryViewModel? payload = JsonSerializer.Deserialize<CreateCategoryViewModel>(Client.PostStream, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            if (payload == null)
            {
                return Fail("输入了错误格式的内容");
            }

            var isExists = categoryService.Categories.Any(e => e.ID == payload.ParentId);
            if (payload.ParentId != Guid.Empty && !isExists)
            {
                return Fail($"找不到ID为{payload.ParentId}的分类");
            }

            CategoryRepository cr = Resolve<CategoryRepository>();

            cr.CreateCategories(Client, payload);

            return Ok(payload);
        }

        [ApiLinkTo("删除栏目", "编辑栏目")]
        public ActionResult RemoveCategory()
        {
            var (entity, ar) = InternalGetCategory();
            if (entity == null)
            {
                return ar;
            }

            bool force = Get("force", false);
            List<Guid> ids = [];
            if (entity.AllChildren.Any() && !force)
            {
                return Ok(new
                {
                    success = false,
                    msg = "无法删除还有子栏目的栏目。"
                });
            }
            else
            {
                ids = entity.AllChildren.Select(e => e.ID).ToList();
                ids.Add(entity.ID);
            }

            CategoryRepository repo = Resolve<CategoryRepository>();
            repo.RemoveIds(Client, entity, ids);

            return Ok();
        }

        [ApiLinkTo("批量编辑栏目", "编辑栏目")]
        public ActionResult MoveCategory()
        {
            Guid to = Get("to", Guid.Empty);
            string opera = Get("opera", "copy");
            Guid srcCategoryId = Get("srcCategoryId", Guid.Empty);
            List<Guid> value = GetList<Guid>("value");

            Category? cate = categoryService.Categories.FirstOrDefault(e => e.ID == to);
            if (to != Guid.Empty && cate == null)
            {
                return Ok(new
                {
                    success = false,
                    msg = "找不到目标栏目",
                    to
                });
            }

            if (to != Guid.Empty && opera == "cut" && categoryService.Categories.Where(e => value.Contains(e.ID)).Any(x => x.ID == to || x.AllChildren.Any(y => y.ID == to)))
            {
                return Ok(new
                {
                    success = false,
                    msg = "剪切的目标位置不能是源位置，也不能在源位置的内部。",
                    to
                });
            }

            if (categoryService.Categories.Any(x => value.Contains(x.ID) && x.CategoryType == CategoryType.Home))
            {
                return Ok(new
                {
                    success = false,
                    msg = $"被处理的栏目中有首页类型的栏目，这种类型的栏目不允许复制，粘贴或者删除。"
                });
            }

            if (value.Count == 0)
            {
                return Ok();
            }

            var (success, msg, affectRows) = categoryService.MoveCategory(srcCategoryId, to, value, opera, Client);

            string toCate = cate?.Name ?? "根栏目";
            string desc = opera switch
            {
                "cut" => $"将id为“{value.JoinAsString("，")}”的栏目剪切至“{toCate}”栏目下",
                "copy" => $"将id为“{value.JoinAsString("，")}”的栏目复制至“{toCate}”栏目下",
                "delete" => $"删除id为“{value.JoinAsString("，")}”的栏目",
                _ => "未知的操作"
            };
            Client.Log(MODULE_NAME, "MoveCategory", to, desc);

            if (opera == "delete")
            {
                List<Guid> affects = affectRows.Select(e => e.ID).ToList();
                // 把被影响的新闻全部处理一遍
                var repo = Resolve<CmsRepository>();
                repo.DbSet
                .Where(e => affects.Contains(e.ID))
                .ExecuteUpdate(set => set
                    .SetProperty(e => e.CategoryId, v => Guid.Empty)
                );
                repo.SaveChanges();
            }

            return Ok(new
            {
                success,
                msg,
            });
        }

        private (Category? cate, ActionResult err) InternalGetCategory(string arg = "id")
        {
            Guid id = Get(arg, Guid.Empty);
            string uniqueCode = Get("code", string.Empty);
            Category? category;

            if (id == Guid.Empty)
            {
                if (string.IsNullOrWhiteSpace(uniqueCode))
                {
                    return (null, Ok(new
                    {
                        success = false,
                        msg = "ID和Code不能同时为空"
                    }));
                }

                category = categoryService.GetCategory(uniqueCode);
                if (category == null)
                {
                    return (null, Ok(new
                    {
                        success = false,
                        msg = $"找不到编号为“{uniqueCode}”的分类"
                    }));
                }
            }
            else
            {
                category = categoryService.GetCategory(id);
                if (category == null)
                {
                    return (null, Ok(new
                    {
                        success = false,
                        msg = $"找不到ID为“{id}”的分类"
                    }));
                }
            }

            return (category, Ok());
        }
    }

    internal record CreateCategoryViewModel(Guid ParentId, MiniCategory Data);
}
