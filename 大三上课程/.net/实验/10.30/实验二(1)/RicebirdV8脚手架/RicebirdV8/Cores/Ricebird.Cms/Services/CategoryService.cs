using Ricebird.Cms.Models;

namespace Ricebird.Cms.Services
{
    public class CategoryService : ISingletonDependency
    {
        private IServiceProvider ServiceProvider { get; set; }

        private readonly object cateLock = new object();
        private List<Category> _cate = new List<Category>();
        public List<Category> Categories
        {
            get
            {
                if (_cate == null || _cate.Count == 0)
                {
                    LoadCategory();
                }

#pragma warning disable CS8603 // 可能返回 null 引用。
                return _cate;
#pragma warning restore CS8603 // 可能返回 null 引用。
            }
        }

        public void LoadCategory()
        {
            lock (cateLock)
            {
                using var scope = ServiceProvider.CreateScope();
                var repo = scope.Resolve<CategoryRepository>();
                List<Category> cate = repo.LoadAllNodes().ToList();

                foreach (var c in cate)
                {
                    c.ParentName = c.Parent?.Name ?? "根栏目";
                }

                _cate = cate;
            }
        }

        public CategoryService(IServiceProvider sp)
        {
            ServiceProvider = sp;
        }

        /// <summary>
        /// 这是给前端用的加载函数
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Category? FirstOrDefault(Func<Category, bool> predicate) => Categories.Where(e => e.CategoryStatus == Visibility.Visiable).FirstOrDefault(predicate);

        public Category? GetCategory(Guid id) => Categories.FirstOrDefault(e => e.ID == id);
        public Category? GetCategory(string uniqueCode) => Categories.FirstOrDefault(e => e.UniqueCode == uniqueCode);

        public Category? GetHome() => Categories.FirstOrDefault(e => e.CategoryType == CategoryType.Home);

        public Category? UniqueCodeValidate(Guid id, string uniqueCode)
        {
            var cate = Categories.FirstOrDefault(e => e.ID != id && e.UniqueCode == uniqueCode);
            return cate;
        }

        /// <summary>
        /// 移动分类
        /// </summary>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="opera"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public (bool success, string msg, List<Category> affectRow) MoveCategory(Guid srcCategoryId, Guid to, List<Guid> value, string opera, IClient client)
        {
            CategoryRepository cr = client.Resolve<CategoryRepository>();

            var ans = cr.MoveCategories(client, srcCategoryId, to, value, opera);
            if (ans.success)
            {
                LoadCategory();
            }

            return ans;
        }
    }
}
