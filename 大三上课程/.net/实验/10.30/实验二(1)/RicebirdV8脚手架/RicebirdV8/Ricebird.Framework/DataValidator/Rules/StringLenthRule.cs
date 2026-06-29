using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class StringMaxLenthRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;
        public override string RuleName => "字符串长度限制";
        public const string DEFAULTMESSAGE = "该字段最多{1}个字";

        //字符串最大长度限制
        protected int MaxLength
        {
            get; init;
        }

        public StringMaxLenthRule(int lenth)
        {
            Message = DEFAULTMESSAGE;
            MaxLength = lenth;
        }

        public StringMaxLenthRule(int lenth, string message)
        {
            MaxLength = lenth;
            Message = message;
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {

                if (value?.ToString()?.Length > MaxLength)
                {
                    result.SetFailure(propertyName, string.Format(Message, fieldName, MaxLength));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public override object ToJsonObject(string fieldName)
        {
            return new
            {
                type = "string",
                min = 0,
                max = MaxLength,
                message = string.Format(Message, fieldName, MaxLength)
            };
        }
    }

    public class StringLenthRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;
        public override string RuleName => "字符串长度限制";
        public const string DEFAULTMESSAGE = "该字段必须是{1}个字";

        //字符串长度限制
        protected int Length
        {
            get; init;
        }

        public StringLenthRule(int lenth)
        {
            Message = DEFAULTMESSAGE;
            Length = lenth;
        }

        public StringLenthRule(int lenth, string message)
        {
            Length = lenth;
            Message = message;
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            try
            {

                if (value?.ToString()?.Length != Length)
                {
                    result.SetFailure(propertyName, string.Format(Message, fieldName, Length));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public override object ToJsonObject(string fieldName)
        {
            return new
            {
                type = "string",
                min = Length,
                max = Length,
                message = string.Format(Message, fieldName, Length)
            };
        }
    }
}
