using Ricebird.Framework.Database;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static T? GetServiceFromCollection<T>(this IServiceCollection services) => (T?)services.LastOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance;

        public static HostEnv AddHostEnv(this IServiceCollection collection)
        {
            HostEnv env = new HostEnv();
            EntityTypeBuilderExtensions.HostEnv = env;
            HostEnv.Instance = env;
            env.AddOptions();
            collection.AddSingleton(env);
            env.AddAllModules(collection);
            SequentialGuid.SystemId = env.FrameworkOptions.SystemId;
            return env;
        }
    }
}
