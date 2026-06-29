using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class FunctionalRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => true;

        public override string RuleName => "编程式验证器";

        private ValidateAction<T>? Action { get; set; }

        private ObjectValidateAction<T>? ObjectAction { get; set; }

        private SimpleValidateAction<T>? SimpleAction { get; set; }

        public FunctionalRule(SimpleValidateAction<T> validator)
        {
            SimpleAction = validator;
        }


        public FunctionalRule(ValidateAction<T> validator)
        {
            Action = validator;
        }

        public FunctionalRule(ObjectValidateAction<T> validator)
        {
            ObjectAction = validator;
        }

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            Action?.Invoke(client, result, validateObj, propertyName, fieldName, value);

            ObjectAction?.Invoke(client, result, validateObj);

            SimpleAction?.Invoke(result, validateObj);
        }
    }
}
