using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ricebird.Framework.Clients;

namespace Ricebird.Framework.AspNetCoreExtensions.Binder
{
    public class RicebirdEntityBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            IClient? client = bindingContext.HttpContext.Features.Get<IClient>();
            if (client == null)
            {
                return Task.CompletedTask;
            }

            Type entityType = bindingContext.ModelType;
            var result = client.BindEntity(entityType);
            bindingContext.Result = result;
            return Task.CompletedTask;
        }
    }
}
