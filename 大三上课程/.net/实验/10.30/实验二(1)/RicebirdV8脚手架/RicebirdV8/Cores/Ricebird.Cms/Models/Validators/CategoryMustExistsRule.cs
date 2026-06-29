using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Cms.Models.Validators
{
    internal class CategoryMustExistsRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;

        public override string RuleName => "栏目必须存在";

        public bool AllowGuidEmpty
        {
            get;
            set;
        } = false;

        public CategoryMustExistsRule()
        {

        }

        public CategoryMustExistsRule(bool allowGuidEmpty)
        {
            AllowGuidEmpty = allowGuidEmpty;
        }

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {
                if (value is Guid g)
                {
                    if (g == Guid.Empty && AllowGuidEmpty)
                    {
                        return;
                    }

                    var cs = client.Resolve<CategoryService>();
                    var category = cs.GetCategory(g);

                    if (category == null)
                    {
                        result.SetFailure(propertyName, $"ID为{value}的栏目不存在");
                    }

                    return;
                }

                result.SetFailure(propertyName, $"值{value}不是一个合法的GUID");
            }
            catch
            {
                result.SetFailure(propertyName, $"找不到ID为{value}的栏目");
            }
        }
    }
}
