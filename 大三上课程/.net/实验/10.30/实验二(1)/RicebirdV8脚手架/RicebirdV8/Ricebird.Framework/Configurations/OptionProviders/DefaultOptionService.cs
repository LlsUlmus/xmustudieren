
namespace Ricebird.Framework.Configurations.OptionProviders
{
    internal class DefaultOptionService(IServiceProvider p, IMemoryCache cache) : IOptionService
    {
        private readonly IServiceProvider provider = p;
        private readonly IMemoryCache cache = cache;

        public T LoadOptions<T>()
            where T : IOption, new()
        {
            T opt = new T();
            return LoadOptions(opt);
        }

        public T LoadOptions<T>(T opt)
           where T : IOption, new()
        {
            if (!TryGetCache(out opt))
            {
                using var scope = provider.CreateScope();
                IEnumerable<IOptionStore> stores = scope.Resolve<IEnumerable<IOptionStore>>();
                var store = stores.FirstOrDefault(e => e.SaveTo == opt.OptionSaveTo) ?? throw new NotSupportedException("现在系统不支持这一种选项读取");
                opt = store.LoadOptions(opt);
                CacheOption(opt);
            }

            return opt;
        }

        public void SaveOptions<T>(T opt)
            where T : IOption, new()
        {
            using var scope = provider.CreateScope();
            IEnumerable<IOptionStore> stores = scope.Resolve<IEnumerable<IOptionStore>>();
            var store = stores.FirstOrDefault(e => e.SaveTo == opt.OptionSaveTo) ?? throw new NotSupportedException("现在系统不支持这一种选项读取");
            store.SaveOptions(opt);
            CacheOption(opt);
        }

        protected void CacheOption<T>(T opt)
            where T : IOption, new()
        {
            cache.SetSlider(typeof(T), opt, TimeSpan.FromMinutes(5));
        }

        protected bool TryGetCache<T>(out T opt)
            where T : IOption, new()
        {
            if (!cache.TryGetValue(typeof(T), out T? option))
            {
                opt = new T();
                return false;
            }

            opt = option!;
            return true;
        }
    }
}
