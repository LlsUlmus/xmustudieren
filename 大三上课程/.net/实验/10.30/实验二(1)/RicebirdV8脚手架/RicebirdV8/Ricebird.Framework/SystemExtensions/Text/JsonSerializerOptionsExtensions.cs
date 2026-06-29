namespace Ricebird.Framework
{
    public static class JsonSerializerOptionsExtensions
    {
        public static void AddConverter<T>(this JsonSerializerOptions opt)
            where T : JsonConverter, new()
        {
            opt.Converters.Add(new T());
        }
    }
}
