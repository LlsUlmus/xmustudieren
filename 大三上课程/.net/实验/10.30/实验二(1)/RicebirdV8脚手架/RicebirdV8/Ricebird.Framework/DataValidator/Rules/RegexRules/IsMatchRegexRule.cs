using Ricebird.Framework.Clients;
using System.Text.RegularExpressions;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsMatchRegexRule<T> : AbstactValidateRule<T>
    {
        public IsMatchRegexRule(string regexStr)
        {
            DEFAULTMESSAGE = "{0}必须满足正则表达式" + regexStr;
            Message = DEFAULTMESSAGE;
            REGEX_STR = regexStr;
            Regex = new Regex(REGEX_STR);
        }

        public IsMatchRegexRule(string regexStr, string message)
            : this(regexStr)
        {
            Message = message;
        }

        public override bool Multiple => true;

        public override string RuleName => "必须满足正则表达式";

        public virtual string DEFAULTMESSAGE
        {
            get;
            init;
        }

        public virtual string REGEX_STR
        {
            get;
            init;
        }

        public virtual Regex Regex
        {
            get;
            init;
        }

        public override object ToJsonObject(string fieldName)
        {
            return new { type = "string", pattern = REGEX_STR, message = string.Format(Message, fieldName) };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            if (!string.IsNullOrWhiteSpace(value?.ToString() ?? "") && !Regex.IsMatch(value?.ToString() ?? ""))
            {
                result.SetFailure(propertyName, string.Format(Message, fieldName));
            }
        }
    }
}
