using Ricebird.Framework.Clients;

/// <summary>
/// 由于后端是强类型的，所以不需要验证。这是个给前端使用的验证器
/// </summary>
namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsDateRule<T> : AbstractTypeValidateRule<T>
    {
        public IsDateRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsDateRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为日期";

        public const string DEFAULTMESSAGE = "{0}必须为日期";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "date", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            //if (!DateTime.TryParse(value?.ToString() ?? "", out _))
            //{
            //    result.SetFailure(propertyName, string.Format(Message, fieldName));
            //}
        }
    }
}
