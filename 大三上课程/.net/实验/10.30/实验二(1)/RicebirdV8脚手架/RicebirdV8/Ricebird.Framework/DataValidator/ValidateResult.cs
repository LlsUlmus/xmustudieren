namespace Ricebird.Framework.DataValidator
{
    public class ValidateResult(object obj)
    {
        public object ValidateObject
        {
            get; private set;
        } = obj;

        public bool IsValid
        {
            get; private set;
        } = true;

        public Dictionary<string, List<string>> Errors
        {
            get;
            init;
        } = [];

        public List<string> ErrorStrings
        {
            get;
            init;
        } = [];

        internal string CurrentProperty
        {
            get; set;
        } = string.Empty;

        public ValidateResult SetFailure(string message)
        {
            return SetFailure("", message);
        }

        public ValidateResult SetFailure(string propertyName, string message)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = CurrentProperty;
            }

            IsValid = false;
            if (Errors.TryGetValue(propertyName, out List<string>? errs))
            {
                errs ??= [];
                errs.Add(message);
            }
            else
            {
                Errors.Add(propertyName, [message]);
            }

            ErrorStrings.Add(message);

            return this;
        }

        public ValidateResult MergeResult(ValidateResult result)
        {
            IsValid &= result.IsValid;
            Errors.MergeDictionary(result.Errors);
            ErrorStrings.AddRange(result.ErrorStrings);
            return this;
        }

        public static implicit operator bool(ValidateResult result)
        {
            return result != null && result.IsValid;
        }
    }
}
