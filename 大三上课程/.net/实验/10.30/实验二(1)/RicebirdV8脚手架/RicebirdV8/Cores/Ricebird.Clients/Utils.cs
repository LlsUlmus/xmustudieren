using System.Text;
using System.Text.Json;

namespace Ricebird.Clients
{
    public static class Utils
    {
        public static Dictionary<string, object> ReadJsonLv1(byte[] json)
        {
            Utf8JsonReader reader = new Utf8JsonReader(json);

            Dictionary<string, object> simple = [];
            string currentKey = "";
            StringBuilder subBuilder = new StringBuilder();
            while (reader.Read())
            {
                var token = reader.TokenType;

                switch (token)
                {
                    case JsonTokenType.PropertyName:
                        if (reader.CurrentDepth == 1)
                        {
                            currentKey = reader.GetString() ?? "unknown";
                        }
                        break;
                    case JsonTokenType.Comment:
                        break;
                    case JsonTokenType.String:
                        if (reader.CurrentDepth == 1)
                        {
                            simple.Add(currentKey, reader.GetString() ?? "unknown");
                        }
                        break;
                    case JsonTokenType.Number:
                        if (reader.CurrentDepth == 1)
                        {
                            if (reader.TryGetByte(out byte b))
                            {
                                simple.Add(currentKey, b);
                                break;
                            }

                            if (reader.TryGetInt32(out int v))
                            {
                                simple.Add(currentKey, v);
                                break;
                            }

                            if (reader.TryGetInt64(out long l))
                            {
                                simple.Add(currentKey, l);
                                break;
                            }

                            if (reader.TryGetDouble(out double d))
                            {
                                simple.Add(currentKey, d);
                                break;
                            }
                        }
                        break;
                    case JsonTokenType.True:
                        if (reader.CurrentDepth == 1)
                        {
                            simple.Add(currentKey, true);
                        }
                        break;
                    case JsonTokenType.False:
                        if (reader.CurrentDepth == 1)
                        {
                            simple.Add(currentKey, false);
                        }
                        break;
                    case JsonTokenType.None:
                    case JsonTokenType.StartObject:
                    case JsonTokenType.EndObject:
                    case JsonTokenType.StartArray:
                    case JsonTokenType.EndArray:
                    case JsonTokenType.Null:
                    default:
                        break;
                }
            }

            return simple;
        }
    }
}
