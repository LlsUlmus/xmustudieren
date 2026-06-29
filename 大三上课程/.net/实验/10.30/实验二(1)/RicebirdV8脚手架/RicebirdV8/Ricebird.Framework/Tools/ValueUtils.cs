using Ricebird.Framework.Database.Structures;
using Ricebird.Framework.Tools.ValueConverter;

namespace Ricebird.Framework
{
    /// <summary>
    /// 用于类型转换的工具类
    /// </summary>
    public static class ValueUtils
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        private static IServiceProvider _provider;
        public static IServiceProvider ServiceProvider
        {
            get => _provider;
            set
            {
                _provider = value;
                ConverterProvider = new ConverterProvider(value);
            }
        }

        public static ConverterProvider ConverterProvider
        {
            get; set;
        }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue
        /// </summary>
        /// <typeparam name="T">需转换的类型</typeparam>
        /// <param name="value">待转换的数据</param>
        /// <returns>转换后的数据</returns>
        public static T? ChangeToType<T>(object? value)
        {
            T v = EnsureNotNull(default(T) ?? GetDefaultValue<T>(), $"类型{typeof(T)}的默认值为null，必须给定默认值");
            return ChangeToType<T>(value, v);
        }

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue
        /// </summary>
        /// <typeparam name="T">需转换的类型参数</typeparam>
        /// <param name="value">待转换的数据</param>
        /// <param name="defalutValue">无法转换时需返回的默认值</param>
        /// <returns>转换后的数据</returns>
        public static T? ChangeToType<T>(object? value, T defalutValue)
        {
            if (value == null) return default;
            T val = (T)ChangeToType(value, typeof(T), defalutValue!);
            return val;
        }

        public static IEnumerable<toT> ChangeToType<T, toT>(this IEnumerable<T> src, toT? defaultValue)
        {
            foreach (T item in src)
            {
                var v = ChangeToType(item, defaultValue);
                if (v == null)
                {
                    continue;
                }
                yield return v;
            }
        }

        public static object ChangeToType(object? value, Type T, [NotNull] object defaultValue)
        {
            bool hasDefaultValue = false;
            if (T.IsInstanceOfType(defaultValue))
            {
                hasDefaultValue = true;
            }

            if (value != null)
            {
                System.Type typeFromHandle = T;
                if (typeFromHandle.IsInterface || (typeFromHandle.IsClass && typeFromHandle != typeof(string) && typeFromHandle != typeof(DateRange)))
                {
                    if (T.IsInstanceOfType(value))
                    {
                        return value;
                    }
                }
                else if (T.IsValueType && string.IsNullOrWhiteSpace(value.ToString()))
                {
                    if (!T.IsAssignableFrom(defaultValue.GetType()))
                    {
                        defaultValue = Activator.CreateInstance(T) ?? defaultValue;
                    }
                    return defaultValue;
                }
                else if (T == typeof(string) && value != null)
                {
                    return value.ToString() ?? defaultValue;
                }
                else if (ConverterProvider.TryConvert(value!, T, defaultValue, out object result))
                {
                    return result;
                }
            }

            if (!hasDefaultValue)
            {
                throw new FormatException($"无法将{defaultValue}转换为{T}");
            }

            throw new FormatException($"无法将{value}转换为{T}");
            // return defaultValue;
        }

        public static T ConvertTo<T>(this object value, T defaultValue)
        {
            return ChangeToType(value, defaultValue)!;
        }

        public static string ConvertTo(this object value, string defaultValue)
        {
            return ChangeToType(value, defaultValue)!.Trim();
        }

        public static T GetDefaultValue<T>()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)false;
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (T)(object)0;
                case TypeCode.DateTime:
                    return (T)(object)ConstKeys.MinDate;
                case TypeCode.Char:
                    return (T)(object)'\0';
                case TypeCode.String:
                    return (T)(object)string.Empty;
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                default:
                    throw new NotSupportedException($"无法获取类型{typeof(T).FullName}的默认值");
            }
        }
    }
}
