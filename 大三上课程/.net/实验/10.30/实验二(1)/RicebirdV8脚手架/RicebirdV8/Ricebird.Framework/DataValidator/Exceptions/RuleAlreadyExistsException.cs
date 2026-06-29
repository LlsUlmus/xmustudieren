namespace Ricebird.Framework.DataValidator.Exceptions
{
    public class RuleAlreadyExistsException(Type type, string validator) : Exception($"在类型“{type.Name}”中，已经存在“{validator}”验证器")
    {
    }
}
