using Microsoft.AspNetCore.Http.Features;
using System.Collections;

namespace Ricebird.Clients.Services
{
    internal class ClientFeatureCollection(DefaultClient c) : IFeatureCollection
    {
        public DefaultClient Client { get; set; } = c;

        private readonly IDictionary<Type, object> Features = new Dictionary<Type, object>();

        public bool IsReadOnly => false;

        public int Revision => 0;

        public object? this[Type key]
        {
            get
            {
                object? f = Features.TryGetValue(key, out object? value) ? value : default;
                return f;
            }
            set
            {

            }
        }

        public TFeature? Get<TFeature>()
        {
            TFeature? f;
            if (Client.HttpContext != null)
            {
                f = Client.HttpContext.Features.Get<TFeature>();
                if (f != null) return f;
            }

            f = Features.ContainsKey(typeof(TFeature)) ? (TFeature)Features[typeof(TFeature)] : default;
            return f;
        }

        public void Set<TFeature>(TFeature? instance)
        {
            if (instance != null)
            {
                if (Client.HttpContext == null)
                {
                    Features.MergeKey(typeof(TFeature), instance);
                }
                else
                {
                    Client.HttpContext?.Features.Set<TFeature>(instance);
                }
            }
        }

        public void Dispose()
        {
            Features.Clear();
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            Client = null;
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => Features.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
