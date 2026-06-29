using Ricebird.Framework.Database;
using Ricebird.Framework.DataValidator.Rules;
using Ricebird.Framework.DataValidator.Rules.RegexRules;
using Ricebird.Framework.DataValidator.Rules.TreeRules;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Ricebird.Framework.DataValidator
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
{
    public static class Extensions
    {
        /// <summary>
        /// 验证是否是性别，只能是男或女
        /// 默认的错误消息是："性别只能是男或女"。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> IsGender<T>(this ValidateRules<T> rules)
        {
            rules.AddRule(new IsGenderRule<T>());
            return rules;
        }

        /// <summary>
        /// 验证是否是电子邮箱
        /// 默认的错误消息是："{0}必须为电子邮箱"。其中{0}为字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> IsEmail<T>(this ValidateRules<T> rules)
        {
            rules.AddRule(new IsEmailRule<T>());
            return rules;
        }

        /// <summary>
        /// 验证是否是电子邮箱
        /// 默认的错误消息是："{0}必须为电子邮箱"。其中{0}为字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> IsEmail<T>(this ValidateRules<T> rules, string message)
        {
            rules.AddRule(new IsEmailRule<T>(message));
            return rules;
        }

        /// <summary>
        /// 验证是否是手机号
        /// 默认的错误消息是："{0}必须为手机号"。其中{0}为字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> IsMobile<T>(this ValidateRules<T> rules)
        {
            rules.AddRule(new IsMobileRule<T>());
            return rules;
        }

        /// <summary>
        /// 验证是否是手机号
        /// 默认的错误消息是："{0}必须为手机号"。其中{0}为字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> IsMobile<T>(this ValidateRules<T> rules, string message)
        {
            rules.AddRule(new IsMobileRule<T>(message));
            return rules;
        }
        /// <summary>
        /// 验证字符串长度是否大于指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> MaxLength<T>(this ValidateRules<T> rules, int maxLength)
        {
            rules.AddRule(new StringMaxLenthRule<T>(maxLength));
            return rules;
        }

        /// <summary>
        /// 验证字符串长度必须是指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> StringLength<T>(this ValidateRules<T> rules, int length)
        {
            rules.AddRule(new StringLenthRule<T>(length));
            return rules;
        }

        public static ValidateRules<T> MustInDict<T>(this ValidateRules<T> rules, string dictName)
        {
            rules.AddRule(new DictRule<T>(dictName));
            return rules;
        }

        public static ValidateRules<T> MustInEnum<T>(this ValidateRules<T> rules, string enumString) => MustInEnum(rules, [enumString], "");

        public static ValidateRules<T> MustInEnum<T, TEnumType>(this ValidateRules<T> rules, IEnumerable<TEnumType> enums) => MustInEnum(rules, enums, "");

        public static ValidateRules<T> MustInEnum<T, TEnumType>(this ValidateRules<T> rules, IEnumerable<TEnumType> enums, string message)
        {
            rules.AddRule(new EnumRule<T, TEnumType>(enums.ToArray(), message));
            return rules;
        }

        public static ValidateRules<T> StringEnum<T>(this ValidateRules<T> rules, IEnumerable<string> enums)
        {
            rules.AddRule(new EnumRule<T, string>(enums.ToArray()));
            return rules;
        }

        public static ValidateRules<T> StringEnum<T>(this ValidateRules<T> rules, params string[] enums)
        {
            rules.AddRule(new EnumRule<T, string>(enums));
            return rules;
        }

        /// <summary>
        /// 对验证器进行直接编程，适用于无法复用的场合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <param name="validator">函数参数是 (result, validateObj)</param>
        /// <returns></returns>
        public static ValidateRules<T> Should<T>(this ValidateRules<T> rules, SimpleValidateAction<T> validator)
        {
            rules.AddRule(new FunctionalRule<T>(validator));
            return rules;
        }

        /// <summary>
        /// 对验证器进行直接编程，适用于无法复用的场合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <param name="validator">函数参数是 (client, result, validateObj, propertyName, fieldName, value)</param>
        /// <returns></returns>
        public static ValidateRules<T> Should<T>(this ValidateRules<T> rules, ValidateAction<T> validator)
        {
            rules.AddRule(new FunctionalRule<T>(validator));
            return rules;
        }

        /// <summary>
        /// 对验证器进行直接编程，适用于无法复用的场合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <param name="validator">函数参数是 (client, result, validateObj)</param>
        /// <returns></returns>
        public static ValidateRules<T> Should<T>(this ValidateRules<T> rules, ObjectValidateAction<T> validator)
        {
            rules.AddRule(new FunctionalRule<T>(validator));
            return rules;
        }

        /// <summary>
        /// 只适用于字符串类型的字段，直接去掉头尾的空格
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> ShouldTrim<T>(this ValidateRules<T> rules)
        {
            rules.Should((c, r, o, propertyName, fieldName, value) =>
            {
                if (o != null && value is string str)
                {
                    o.SetPropertyValue(propertyName, str.Trim());
                }
            });
            return rules;
        }

        /// <summary>
        /// message格式化的第一个参数是value, 第二个参数是字段名
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="message">示例：ID为{0}的部门不存在</param>
        /// <param name="allowGuidEmpty"></param>
        /// <returns></returns>
        public static ValidateRules<T> ParentIdMustExists<T>(this ValidateRules<T> rules, string message, bool allowGuidEmpty = true)
            where T : TreeEntityBase<T>, new()
        {
            rules.AddRule(new ParentIdMustExistsRule<T>(message, allowGuidEmpty));
            return rules;
        }
    }
}
