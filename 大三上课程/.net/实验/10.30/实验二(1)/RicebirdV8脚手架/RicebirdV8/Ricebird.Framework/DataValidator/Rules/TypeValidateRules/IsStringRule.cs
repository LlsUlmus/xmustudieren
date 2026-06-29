using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules.TypeValidationRules
{
    /// <summary>
    /// 这是一个永远都不可能用到的验证器
    /// 因为，如果不是string结构的内容就不应该输入到这里。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IsStringRule<T> : AbstractTypeValidateRule<T>
    {
        public override Type ForType => typeof(string);

        public override bool Multiple => false;

        public override string RuleName => "必须为字符串";

        public const string DEFAULTMESSAGE = "{0}必须为字符串";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "string", message = "" };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            // throw new NotImplementedException();
        }
    }
}
