using Ricebird.Framework.Clients;

/// <summary>
/// 由于后端是强类型的，所以不需要验证。这是个给前端使用的验证器
/// </summary>
namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsGuidRule<T> : AbstractTypeValidateRule<T>
    {
        public IsGuidRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsGuidRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为Guid";

        public const string DEFAULTMESSAGE = "{0}必须为Guid";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "string", pattern = "[a-fA-F0-9]{8}(-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }
}
