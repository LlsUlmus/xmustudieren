using Ricebird.Framework.Clients;

namespace System
{
    public static class IServiceProviderExtensions
    {
        public static T Resolve<T>(this IServiceScope scope)
            where T : class => scope.ServiceProvider.Resolve<T>();

        public static T Resolve<T>(this IServiceProvider service)
            where T : class => service.GetService(typeof(T)) is not T s ? throw new InvalidOperationException($"找不到名为{typeof(T)}的服务") : s;

        /// <summary>
        /// 使用工厂模式创建对象，产品名无视大小写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static T Resolve<T>(this IServiceProvider service, string productName)
        {
            T? instance = service.GetKeyedService<T>(productName);
            if (instance != null) return instance;

            IEnumerable<T> instances = service.GetServices<T>();

            foreach (T item in instances)
            {
                if (item != null && item.GetType().Name.Equals(productName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item;
                }
            }

            throw new NotSupportedException($"找不到名为{typeof(T)}的服务");
        }

        public static IClient CreateClient(this IServiceScope scope, string serviceName)
        {
            var provider = scope.Resolve<IClientProvider>();
            var env = scope.Resolve<HostEnv>();
            return provider.CreateClient(scope, env.FrameworkOptions.SystemId, serviceName);
        }
    }
}
