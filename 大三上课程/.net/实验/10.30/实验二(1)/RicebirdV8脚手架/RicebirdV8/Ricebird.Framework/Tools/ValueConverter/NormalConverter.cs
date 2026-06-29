

using Ricebird.Framework.Database.Structures;
using Ricebird.Framework.SystemExtensions;

namespace Ricebird.Framework.Tools.ValueConverter
{
    public class EnumConverter : IConverter
    {
        public int Order => 10;

        public bool IsSuit(Type toType, object fromValue) => toType.IsEnum;

        public object Convert(object fromValue, Type toType, object defaultValue) => Enum.Parse(toType, fromValue?.ToString() ?? "");
    }

    public class GuidConverter : IConverter
    {
        public int Order => 100;

        public bool IsSuit(Type toType, object fromValue) => toType == typeof(Guid) || toType == typeof(SequentialGuid);

        public object Convert(object fromValue, Type toType, object defaultValue)
        {
            string str = fromValue.ToString() ?? "";
            if (Guid.TryParse(str, out Guid guid))
            {
                return guid;
            }

            if (str.TryParseToGuid(out guid))
            {
                return guid;
            }

            if (defaultValue is not Guid)
            {
                defaultValue = Guid.Empty;
            }

            return (Guid)defaultValue;
        }
    }

    public class DateTimeConverter : IConverter
    {
        public int Order => 110;

        public bool IsSuit(Type toType, object fromValue) => toType == typeof(DateTime);

        public object Convert(object fromValue, Type toType, object defaultValue)
        {
            string value = fromValue.ToString() ?? "";
            if (DateTime.TryParse(value.ToString(), out DateTime dt))
            {
                return dt;
            }

            if (int.TryParse(value, out var timestamp))
            {
                return FromUnixMillis(timestamp);
            }

            if (defaultValue is not DateTime)
            {
                defaultValue = DateTime.Now;
            }

            return (DateTime)defaultValue;
        }
    }

    public class BooleanConverter : IConverter
    {
        public int Order => 120;

        public bool IsSuit(Type toType, object fromValue) => toType == typeof(bool);

        public object Convert(object fromValue, Type toType, object defaultValue)
        {
            string? str = fromValue?.ToString() ?? "";
            if (bool.TryParse(str, out bool b))
            {
                return b;
            }

            return str switch
            {
                "1" => true,
                "0" => false,
                _ => (object)string.IsNullOrWhiteSpace(str),
            };
        }
    }

    public class DateRangeCovnerter : IConverter
    {
        public int Order => 130;

        public bool IsSuit(Type toType, object fromValue) => toType == typeof(DateRange);

        public object Convert(object fromValue, Type toType, object defaultValue)
        {
            string v = fromValue?.ToString() ?? "";

            if (DateRange.TryPase(v, out DateRange dateRange))
            {
                return dateRange;
            }

            return defaultValue;
        }
    }

    public class Parserable : IConverter
    {
        private static readonly object locker = new object();
        public int Order => 140;

        private readonly Dictionary<Type, MethodInfo> TryParser = [];

        public bool IsSuit(Type toType, object fromValue) => TryParser.ContainsKey(toType) || toType.GetMethods().Any(e => e.Name == "TryParse");

        public object Convert(object fromValue, Type toType, object defaultValue)
        {
            string? str = fromValue.ToString();

            MethodInfo? tryParseFunc;
            lock (locker)
            {
                if (!TryParser.TryGetValue(toType, out tryParseFunc))
                {
                    tryParseFunc = toType.GetMethods().First(e =>
                    {
                        var param = e.GetParameters();
                        return e.Name == "TryParse" && param.Length == 2 && param[0].ParameterType == typeof(string);
                    });
                    TryParser.Add(toType, tryParseFunc);
                }
            }

            if (!defaultValue.GetType().IsAssignableTo(toType))
            {
                defaultValue = toType.GetDefaultValue()!;
            }
            object?[] param = [str, defaultValue];
            tryParseFunc.Invoke(null, param);

            return param[1]!;
        }
    }
}
