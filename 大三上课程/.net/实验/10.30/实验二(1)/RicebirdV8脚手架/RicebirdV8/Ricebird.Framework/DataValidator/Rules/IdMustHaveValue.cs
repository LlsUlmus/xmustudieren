using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class IdMustHaveValue<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;

        public override string RuleName => "ID不能为空值";

        public override object ToJsonObject(string? fieldName)
        {
            return new { };
        }

        public override void Validate(IClient? client, ValidateResult result, T? validateObj, string propertyName, string? fieldName, object? value)
        {
            if (propertyName.Equals("id", StringComparison.CurrentCultureIgnoreCase) && value is Guid g && g == Guid.Empty)
            {
                result.SetFailure(propertyName, $"ID不能为空值");
            }
        }
    }
}
