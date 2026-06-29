using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Cms.Models.Validators
{
    internal class HomeMustAtRootRule : AbstactValidateRule<Category>
    {
        public override bool Multiple => false;

        public override string RuleName => "主页必须位于根目录下";

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, Category validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {
                var categoryService = client.Resolve<CategoryService>();

                if (validateObj.CategoryType == CategoryType.Home && validateObj.ParentId != Guid.Empty)
                {
                    result.SetFailure(propertyName, "主页必须位于根目录下");
                }
            }
            catch
            {
                result.SetFailure(propertyName, $"在检查{value}的值时发生异常");
            }
        }
    }
}
