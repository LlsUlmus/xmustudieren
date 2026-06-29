namespace Ricebird.Framework.Tools.ValueConverter
{
    public class ConverterProvider(IServiceProvider provider)
    {
        private List<IConverter> Converters { get; set; } = [.. provider.Resolve<IEnumerable<IConverter>>().OrderBy(e => e.Order)];

        public bool TryConvert(object fromValue, Type toType, object defaultValue, out object result)
        {
            foreach (var converter in Converters)
            {
                if (converter.IsSuit(toType, fromValue))
                {
                    result = converter.Convert(fromValue, toType, defaultValue);
                    return true;
                }
            }

            result = new object();
            return false;
        }
    }
}
