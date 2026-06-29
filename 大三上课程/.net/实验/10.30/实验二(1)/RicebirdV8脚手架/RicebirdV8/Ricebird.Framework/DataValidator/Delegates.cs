using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator
{
    public delegate void ValidateAction<T>(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value);
    public delegate void ObjectValidateAction<T>(IClient client, ValidateResult result, T validateObj);
    public delegate void SimpleValidateAction<T>(ValidateResult result, T validateObj);
}
