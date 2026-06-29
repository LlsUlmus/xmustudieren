using Ricebird.Framework.Clients;
using Ricebird.Framework.DataValidator;

namespace Ricebird.Framework
{
    public static class IValidatableExtensions
    {
        public static ValidateResult Validate(this IValidatable entity, IClient client)
        {
            var validator = entity.BuildValidator();
            if (validator == null)
            {
                return new ValidateResult(entity);
            }

            var result = validator.Validate(entity, client);
            return result;
        }
    }
}
