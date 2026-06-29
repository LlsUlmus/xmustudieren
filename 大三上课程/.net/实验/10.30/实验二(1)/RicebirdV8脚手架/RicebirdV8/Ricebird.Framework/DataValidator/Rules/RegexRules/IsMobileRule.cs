namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsMobileRule<T> : IsMatchRegexRule<T>
    {
        public IsMobileRule() : base(@"1[3-9]\d{9}", "必须为手机号")
        {
        }

        public IsMobileRule(string message) : base(@"1[3-9]\d{9}", message)
        {
        }
    }
}
