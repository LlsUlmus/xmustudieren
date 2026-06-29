using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class DateLimit<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;
        public override string RuleName => "日期上下限检查";
        public const string DEFAULTMESSAGE = "日期必须介于上下限之间{0}";

        protected DateTime MAXDT, MINDT;

        public DateLimit()
        {
            Message = DEFAULTMESSAGE;
        }

        public DateLimit(DateTime up, DateTime down, string message)
        {
            MAXDT = up;
            MINDT = down;
            Message = message;
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            if (value == null)
            {
                result.SetFailure(propertyName, $"字段{propertyName}不可为null");
                return;
            }

            if (DateTime.Compare((DateTime)value, MAXDT) == -1 && DateTime.Compare((DateTime)value, MINDT) == 1)
            {
                result.SetFailure(propertyName, string.Format(Message, fieldName));
            }
        }

        public override object ToJsonObject(string fieldName)
        {
            return new
            {
                dateLimit = true,
                message = string.Format(Message, fieldName)
            };
        }
    }
}
