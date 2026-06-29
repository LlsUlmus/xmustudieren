using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsEnumRule<T> : AbstactValidateRule<T>
    {
        public IsEnumRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsEnumRule(string message)
        {
            Message = message;
        }

        public override bool Multiple => false;

        public override string RuleName => "必须为枚举型";

        public const string DEFAULTMESSAGE = "{0}只能为枚举型指定的内容";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "number", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }
}
