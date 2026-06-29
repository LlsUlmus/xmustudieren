using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ricebird.Framework.Database;

namespace Ricebird.Framework.AspNetCoreExtensions.Binder
{
    public class RicebirdEntityBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Metadata.ModelType.IsAssignableTo(typeof(EntityBase)) ? new RicebirdEntityBinder() : (IModelBinder?)null;
        }
    }
}
