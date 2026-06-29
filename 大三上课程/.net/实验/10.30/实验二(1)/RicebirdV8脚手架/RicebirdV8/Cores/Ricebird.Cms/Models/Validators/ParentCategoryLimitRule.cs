using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Cms.Models.Validators
{
    internal class ParentCategoryLimitRule : AbstactValidateRule<Category>
    {
        public override bool Multiple => false;

        public override string RuleName => "上级类型限制";

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, Category validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {
                if (validateObj.ParentId == Guid.Empty)
                {
                    return;
                }

                var categoryService = client.Resolve<CategoryService>();
                var cate = categoryService.GetCategory(validateObj.ParentId);
                if (cate == null)
                {
                    throw new Exception("在此处，栏目不应该为空");
                }
                CategoryType[] allowChidren =
                {
                    CategoryType.Home,
                    CategoryType.Normal,
                    CategoryType.Link
                };

                if (!allowChidren.Contains(cate.CategoryType))
                {
                    result.SetFailure(propertyName, $"不允许在栏目“{cate.Name}”下新建子栏目");
                }
            }
            catch
            {
                result.SetFailure(propertyName, $"在检查{value}的值时发生异常");
            }
        }
    }
}
