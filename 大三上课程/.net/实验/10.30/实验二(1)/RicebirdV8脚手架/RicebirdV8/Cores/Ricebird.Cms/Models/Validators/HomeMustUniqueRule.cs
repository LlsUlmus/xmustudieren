using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Cms.Models.Validators
{
    internal class HomeMustUniqueRule : AbstactValidateRule<Category>
    {
        public override bool Multiple => false;

        public override string RuleName => "全系统只能有一个主页";

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, Category validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {
                var categoryService = client.Resolve<CategoryService>();
                var home = categoryService.GetHome();
                if (validateObj.CategoryType == CategoryType.Home && home != null && home.ID != validateObj.ID)
                {
                    result.SetFailure(propertyName, "在系统中已经有一个主页了");
                }
            }
            catch
            {
                result.SetFailure(propertyName, $"在检查{value}的值时发生异常");
            }
        }
    }
}
