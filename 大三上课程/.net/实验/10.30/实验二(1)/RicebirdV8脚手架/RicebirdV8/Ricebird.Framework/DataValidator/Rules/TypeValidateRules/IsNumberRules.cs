using Ricebird.Framework.Clients;

/// <summary>
/// 这几个验证器都是给前端用的，由于后端是强类型的，所以根本就不用验证
/// </summary>
namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsIntRule<T> : AbstractTypeValidateRule<T>
    {
        public IsIntRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsIntRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为数字";

        public const string DEFAULTMESSAGE = "{0}必须为数字";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "integer", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }

    public class IsLongRule<T> : AbstractTypeValidateRule<T>
    {
        public IsLongRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsLongRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为数字";

        public const string DEFAULTMESSAGE = "{0}必须为数字";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "integer", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }

    public class IsDecimalRule<T> : AbstractTypeValidateRule<T>
    {
        public IsDecimalRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsDecimalRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为数字";

        public const string DEFAULTMESSAGE = "{0}必须为数字";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "float", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }

    public class IsFloatRule<T> : AbstractTypeValidateRule<T>
    {
        public IsFloatRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsFloatRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为数字";

        public const string DEFAULTMESSAGE = "{0}必须为数字";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "float", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {

        }
    }

    public class IsDoubleRule<T> : AbstractTypeValidateRule<T>
    {
        public IsDoubleRule()
        {
            Message = DEFAULTMESSAGE;
        }

        public IsDoubleRule(string message)
        {
            Message = message;
        }

        public override Type ForType => typeof(int);

        public override bool Multiple => false;

        public override string RuleName => "必须为数字";

        public const string DEFAULTMESSAGE = "{0}必须为数字";

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "float", message = string.Format(DEFAULTMESSAGE, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            //if (!double.TryParse(value?.ToString() ?? "", out _))
            //{
            //    result.SetFailure(propertyName, string.Format(Message, fieldName));
            //}
        }
    }
}
