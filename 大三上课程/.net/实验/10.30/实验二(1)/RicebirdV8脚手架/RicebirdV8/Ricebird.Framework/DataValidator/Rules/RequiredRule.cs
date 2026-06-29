using Ricebird.Framework.Clients;
namespace Ricebird.Framework.DataValidator.Rules
{
    public class RequiredRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;
        public override string RuleName => "必填";
        public const string DEFAULTMESSAGE = "必须填写此字段";

        public RequiredRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public RequiredRule(string message)
        {
            Message = message;
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                result.SetFailure(propertyName, string.Format(Message, fieldName));
            }
        }

        public override object ToJsonObject(string fieldName)
        {
            return new
            {
                required = true,
                message = string.Format(Message, fieldName)
            };
        }
    }
}
