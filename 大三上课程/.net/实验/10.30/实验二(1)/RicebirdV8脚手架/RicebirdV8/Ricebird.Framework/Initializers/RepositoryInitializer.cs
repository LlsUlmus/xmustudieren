using Ricebird.Framework.Database;

namespace Ricebird.Framework.Initializers
{
    internal class RepositoryInitializer(List<Type> entityTypes)
    {
        private List<Type> EntityTypes
        {
            get; init;
        } = entityTypes;

        public void Initialize(IServiceCollection services)
        {
            foreach (var type in EntityTypes)
            {
                Type baseType = typeof(IRepository<>).MakeGenericType(type);
                Type implementationType = typeof(RepositoryBase<>).MakeGenericType(type);
                services.AddTransient(baseType, implementationType);
                services.AddTransient(implementationType, implementationType);
            }
        }
    }
}
