using System.ComponentModel;
using System.Linq.Expressions;

namespace Ricebird.Framework.DataValidator
{
    internal class PropertyMeta(PropertyInfo member, Delegate exp, Type declaringType, string fieldName) : IEquatable<PropertyMeta>
    {
        public string Name = member.Name;
        public Type ForType = declaringType;
        public Type PropertyType = member.PropertyType;
        public PropertyInfo Property = member;
        public Delegate getter = exp;
        public string FieldName = fieldName;

        public bool Equals(PropertyMeta? other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name && ForType == other.ForType && PropertyType == other.PropertyType;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PropertyMeta);
        }

        public override int GetHashCode()
        {
            return $"{Name}/{ForType.Name}/{PropertyType.Name}".GetHashCode();
        }

        public object? GetData(object @this)
        {
            return getter.DynamicInvoke(@this);
        }

        internal static PropertyMeta GetProperyFromSelector<T, TKey>(Expression<Func<T, TKey>> keySelector, string fieldName)
        {
            if (keySelector.Body is MemberExpression memExpression)
            {
                PropertyInfo? prop = memExpression.Member as PropertyInfo ?? throw new Exception("该表达式必须指定为属性，而不能是成员变量");
                var func = keySelector.Compile();
                fieldName = string.IsNullOrWhiteSpace(fieldName) ? prop.Name : fieldName;

                if (string.IsNullOrEmpty(fieldName))
                {
                    DisplayNameAttribute? display = prop.GetCustomAttribute<DisplayNameAttribute>();
                    fieldName = display != null ? display.DisplayName : "";
                }
                return new PropertyMeta(prop, func, typeof(T), fieldName);
            }
            else
            {
                throw new Exception("该表达式必须为 访问属性 表达式");
            }
        }
    }
}
