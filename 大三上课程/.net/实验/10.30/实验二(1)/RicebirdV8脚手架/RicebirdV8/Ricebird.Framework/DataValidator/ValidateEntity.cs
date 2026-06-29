using Ricebird.Framework.DataValidator;

namespace Ricebird.Framework.Database
{
    public abstract class ValidateEntity : EntityBase, IValidatable
    {
        public abstract FluentValidator BuildValidator();
    }
}
