namespace Ricebird.Framework.DataValidator.Rules.RegexRules
{
    public class IsGenderRule<T> : IsMatchRegexRule<T>
    {
        public IsGenderRule() : base(@"^[男女]{1}$", "性别只能是男或女")
        {

        }

        public IsGenderRule(string message) : base(@"^[男女]{1}$", message)
        {

        }
    }
}
