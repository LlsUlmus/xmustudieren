namespace Ricebird.Framework.DataValidator.Rules
{
    public class IsEmailRule<T> : IsMatchRegexRule<T>
    {
        public IsEmailRule() : base(@"[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+", "必须为电子邮箱")
        {

        }

        public IsEmailRule(string message) : base(@"[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+", message)
        {

        }
    }
}
