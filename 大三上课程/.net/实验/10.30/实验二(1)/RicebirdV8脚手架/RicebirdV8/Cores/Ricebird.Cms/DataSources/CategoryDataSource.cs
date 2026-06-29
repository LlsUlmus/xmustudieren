using Ricebird.Cms.Models;

namespace Ricebird.Cms.DataSources
{
    public class CategoryDataSource(CategoryService categoryService, IContentChangeSource changeSource) : PageDataSource<Category>
    {
        #region 搜索字段
        public Guid ID { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid ParentId { get; set; } = Guid.Empty;
        #endregion

        public override List<Category> GetData()
        {
            if (Result.Count > 0)
            {
                return Result;
            }

            if (ID != Guid.Empty)
            {
                Result = categoryService.Categories.Where(e => e.ID == ID).ToList();
            }
            else if (Name != string.Empty)
            {
                Result = categoryService.Categories.Where(e => e.Name == Name).ToList();
            }
            else
            {
                Result = categoryService.Categories.Where(e => e.ParentId == ParentId && e.CategoryStatus == Visibility.Visiable).ToList();
            }

            ChangeToken = changeSource.BuildChangeToken([], [], [Guid.Empty]);

            Memo = $"""
                分类检索器 @ {DateTime.Now:H时m分s秒}
                """;
            return Result;
        }
    }
}
