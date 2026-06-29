using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Cms.Models.Validators
{
    internal class CodeMustUniqueRule : AbstactValidateRule<Category>
    {
        public override bool Multiple => false;

        public override string RuleName => "全局编号必须唯一";

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, Category validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {
                string uniqueCode = value?.ToString() ?? "";
                if (uniqueCode.IsNullOrWhiteSpace())
                {
                    result.SetFailure(propertyName, $"全局编号不能为空");
                    return;
                }

                var cs = client.Resolve<CategoryService>();
                Category? existCateory = cs.UniqueCodeValidate(validateObj.ID, uniqueCode);
                if (existCateory != null)
                {
                    result.SetFailure(propertyName, $"该全局编号已被名为“{existCateory.Name}”的栏目使用。");
                    return;
                }
            }
            catch
            {
                result.SetFailure(propertyName, $"在检查{value}的值时发生异常");
            }
        }
    }
}
