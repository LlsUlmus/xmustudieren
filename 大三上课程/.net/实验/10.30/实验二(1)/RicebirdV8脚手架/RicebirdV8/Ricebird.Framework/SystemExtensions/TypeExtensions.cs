namespace Ricebird.Framework.SystemExtensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, object> defaultValueDict = new Dictionary<Type, object>()
        {
            [typeof(sbyte)] = 0,
            [typeof(byte)] = 0,
            [typeof(short)] = 0,
            [typeof(ushort)] = 0,
            [typeof(int)] = 0,
            [typeof(uint)] = 0,
            [typeof(long)] = 0,
            [typeof(ulong)] = 0,
            [typeof(nint)] = 0,
            [typeof(nuint)] = 0,
            [typeof(float)] = 0,
            [typeof(double)] = 0,
            [typeof(decimal)] = 0,
            [typeof(bool)] = false,
            [typeof(string)] = "",
            [typeof(char)] = '\0',
            [typeof(Guid)] = Guid.Empty,
            [typeof(DateTime)] = ConstKeys.MinDate,
        };
        public static object? GetDefaultValue(this Type type)
        {
            if (type.IsClass || type.IsInterface || type.IsAbstract)
            {
                return null;
            }

            if (type.IsEnum)
            {
                FieldInfo? field = type.GetFields()?.FirstOrDefault(e => e.IsStatic);

                return field == null ? throw new NotSupportedException($"枚举类型{type}没有任何可用选项") : field.GetValue(null)!;
            }

            if (!defaultValueDict.TryGetValue(type, out object? value))
            {
                throw new NotSupportedException($"获取类型{type}的默认值。");
            }

            return value;
        }
    }
}
