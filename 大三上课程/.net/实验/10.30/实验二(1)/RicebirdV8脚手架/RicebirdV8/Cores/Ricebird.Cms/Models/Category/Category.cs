using Ricebird.Framework.DataValidator;

namespace Ricebird.Cms.Models
{
    internal record MiniCategory(string? Name, CategoryType? CategoryType, string? LinkTo, string? CategoryTemplate, string? ContentTemplate, int? DisplayOrder, List<MiniCategory>? Children);

    public class Category : TreeEntityBase<Category>, IValidatable
    {
        public Category() { }

        internal Category(MiniCategory mini, Guid parentId)
        {
            ParentId = parentId;
            Name = mini.Name ?? "";
            CategoryType = mini.CategoryType ?? CategoryType.Normal;
            LinkTo = mini.LinkTo ?? "";
            CategoryTemplate = mini.CategoryTemplate ?? "";
            ContentTemplate = mini.ContentTemplate ?? "";
            DisplayOrder = mini.DisplayOrder ?? 10;
            DisplayOrder = DisplayOrder == 0 ? 10 : DisplayOrder;
        }

        #region 数据库字段        
        [MaxLength(20)]
        public string Code
        {
            get; set;
        } = string.Empty;

        public bool IsShowOnIndex
        {
            get; set;
        } = true;

        [MaxLength(200)]
        public string SeoKeyword
        {
            get; set;
        } = string.Empty;

        [MaxLength(200)]
        public string SeoDescription
        {
            get; set;
        } = string.Empty;

        public string LinkTo
        {
            get; set;
        } = string.Empty;

        public CategoryType CategoryType
        {
            get; set;
        } = CategoryType.Normal;

        /// <summary>
        /// 标题图文件（带部分路径）
        /// nvarchar(255)
        /// </summary>
        [MaxLength(200)]
        public string FeaturedImage
        {
            get;
            set;
        } = string.Empty;

        public string CategoryTemplate
        {
            get; set;
        } = string.Empty;

        public string ContentTemplate
        {
            get; set;
        } = string.Empty;

        public Visibility CategoryStatus
        {
            get; set;
        } = Visibility.Visiable;

        /// <summary>
        /// 显示在 栏目管理页 的特别信息
        /// </summary>
        public string SpecialMessageInCategory
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 显示在 内容页 的特别信息
        /// </summary>
        public string SpecialMessageInContent
        {
            get; set;
        } = string.Empty;

        public Guid SrcId
        {
            get; set;
        } = Guid.Empty;
        #endregion

        #region 非数据库字段
        [NotMapped]
        public string UniqueCode => ID.To62String();

        [NotMapped]
        public string ParentName
        {
            get; set;
        } = string.Empty;

        public string GetActualLink(int page)
        {
            if (Name == "首页") return "/";
            return $"/cms/{ID.To62String()}-{page}";
        }
        #endregion

        public object ConvertToJson()
        {
            if (Children.Any())
            {
                return new
                {
                    key = ID,
                    id = ID,
                    pid = ParentId,
                    pn = ParentName,
                    display = ParentName == "无" ? Name : $"{ParentName} >> {Name}",
                    title = Name,
                    name = Name,
                    type = CategoryType,
                    children = Children.Select(e => e.ConvertToJson())
                };
            }
            else
            {
                return new
                {
                    key = ID,
                    id = ID,
                    pid = ParentId,
                    pn = ParentName,
                    display = ParentName == "无" ? Name : $"{ParentName} >> {Name}",
                    name = Name,
                    title = Name,
                    type = CategoryType,
                };
            }
        }

        public FluentValidator BuildValidator()
        {
            FluentValidator<Category> fv = new FluentValidator<Category>();
            fv.AutoRulesByAttributes();
            fv.RuleFor(e => e.ParentId).CategoryMustExist(true)
                                       .ParentCategoryLimit();

            if (DisplayOrder == 0 && CategoryType != CategoryType.Home)
            {
                DisplayOrder = 10;
            }

            switch (CategoryType)
            {
                case CategoryType.Normal:
                    break;
                case CategoryType.Link:
                    // fv.RuleFor(e => e.LinkTo).Required();
                    break;
                case CategoryType.Gallery:
                    break;
                case CategoryType.Timeline:
                    break;
                case CategoryType.Home:
                    DisplayOrder = 0;
                    fv.RuleFor(e => CategoryType).HomeMustUnique();
                    fv.RuleFor(e => CategoryType).HomeMustAtRoot();
                    break;
                default:
                    break;
            }

            return fv;
        }
    }
}
