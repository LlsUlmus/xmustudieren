using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public abstract class AbstactValidateRule<T>
    {
        public abstract bool Multiple
        {
            get;
        }

        public abstract string RuleName
        {
            get;
        }

        public string Message
        {
            get; set;
        } = string.Empty;

        public abstract void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value);

        public abstract object ToJsonObject(string fieldName);
    }
}
