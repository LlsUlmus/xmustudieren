using Ricebird.Framework.Database.Structures;

namespace Ricebird.Framework.Tools.JsonConverters
{
    public class DateRangeConverter : JsonConverter<DateRange>
    {
        public override DateRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString()!;
            return str;
        }

        public override void Write(Utf8JsonWriter writer, DateRange value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
