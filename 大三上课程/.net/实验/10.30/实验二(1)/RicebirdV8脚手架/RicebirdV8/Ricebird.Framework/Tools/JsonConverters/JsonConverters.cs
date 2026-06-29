using Ricebird.Framework.Tools.ValueConverter;

namespace Ricebird.Framework.Tools.JsonConverters
{
    public class RicebirdDateTimeConverter(string formatter) : JsonConverter<DateTime>
    {
        private readonly DateTimeConverter converter = new DateTimeConverter();

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString()!;
            return (DateTime)converter.Convert(str, typeToConvert, DateTime.Now);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(formatter));
        }
    }

    public class SystemCodeConverter : JsonConverter<SystemCode>
    {
        public override SystemCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString()!;
            return new SystemCode(str);
        }

        public override void Write(Utf8JsonWriter writer, SystemCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class SequentialGuidConverter : JsonConverter<SequentialGuid>
    {
        public override SequentialGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            GuidConverter converter = new GuidConverter();
            string str = reader.GetString()!;
            return (Guid)converter.Convert(str, typeof(Guid), Guid.Empty);
        }

        public override void Write(Utf8JsonWriter writer, SequentialGuid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class RicebirdGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            GuidConverter converter = new GuidConverter();
            string str = reader.GetString()!;
            return (Guid)converter.Convert(str, typeof(Guid), Guid.Empty);
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
