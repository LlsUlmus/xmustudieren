using Ricebird.Cms.Controllers;
using Ricebird.Cms.DataSources;
using Ricebird.Framework.DataValidator;

namespace Ricebird.Cms.Models
{
    public class CategoryRepository(RicebirdContext ctx, IServiceProvider scoped,
                    IContentChangeSource ccs, CategoryService categoryService) : TreeRepositoryBase<Category>(ctx, scoped)
    {
        private readonly IContentChangeSource changeSource = ccs;
        readonly CategoryService categoryService = categoryService;

        public DbSet<Category> Categories => DbSet;

        public DbSet<Article> Articles => DbContext.Set<Article>();

        public (ValidateResult result, DbOperate opera, Category entity) SaveCategory(IClient client)
        {
            var (opera, entity) = FillEntity(client, ["InternalTreeCode", "ParentName", "UniqueCode"]);

            var result = entity.Validate(client);
            if (!result)
            {
                return (result, DbOperate.None, new Category());
            }

            SaveChanges();

            Task.Run(() =>
            {
                categoryService.LoadCategory();
            });

            client.Log(MODULE_NAME, "SaveCategory", entity, $"保存了分类{entity}");
            changeSource.OnCategoryChanged(client, entity.ID, entity.ParentId);

            return (result, opera, entity);
        }

        internal ValidateResult CreateCategories(IClient client, CreateCategoryViewModel payload)
        {
            MiniCategory current = payload.Data;
            Category cate = new Category(current, payload.ParentId);
            var result = cate.Validate(client);
            if (!result)
            {
                return result;
            }

            List<Guid> affectRows = [];
            Categories.Add(cate);
            affectRows.Add(cate.ID);
            BuildCategory(cate, current.Children, affectRows);
            SaveChanges();

            categoryService.LoadCategory();
            client.Log(MODULE_NAME, "CreateCategories", payload.ParentId, $"创建了一组分类");

            changeSource.OnCategoryChanged(client, [.. affectRows]);

            return result;
        }

        private void BuildCategory(Category parent, List<MiniCategory>? children, List<Guid> affectRows)
        {
            if (children == null || children.Count == 0)
            {
                return;
            }

            Guid parentId = parent.ID;
            foreach (var item in children)
            {
                Category cate = new Category(item, parentId);

                if (cate.CategoryType == CategoryType.Home)
                {
                    continue;
                }

                Categories.Add(cate);
                affectRows.Add(cate.ID);
                BuildCategory(cate, item.Children, affectRows);
            }
        }

        /// <summary>
        /// 删除栏目
        /// </summary>
        /// <param name="client"></param>
        /// <param name="entity">被删除的栏目</param>
        /// <param name="ids">要与其一同被删除的子栏目</param>
        public void RemoveIds(IClient client, Category entity, List<Guid> ids)
        {
            RemoveWhere(e => ids.Contains(e.ID), true);

            Task.Run(() =>
            {
                categoryService.LoadCategory();
            });

            client.Log(MODULE_NAME, "RemoveCategory", entity, $"删除了分类{entity}");

            ids.Add(entity.ParentId);
            changeSource.OnCategoryChanged(client, [.. ids]);
        }

        public (bool success, string msg, List<Category> affectRows) MoveCategories(IClient client, Guid src, Guid to, List<Guid> value, string opera)
        {
            var result = MoveNodes(to, value, opera);

            switch (opera)
            {
                case "delete":
                case "cut":
                    changeSource.OnCategoryChanged(client, to, src);
                    break;
                default:
                    changeSource.OnCategoryChanged(client, to);
                    break;
            }

            return result;
        }
    }
}
