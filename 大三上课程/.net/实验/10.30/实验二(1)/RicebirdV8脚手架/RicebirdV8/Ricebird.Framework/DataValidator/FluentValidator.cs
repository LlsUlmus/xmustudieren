using Ricebird.Framework.Clients;
using Ricebird.Framework.DataValidator.Attributes;
using Ricebird.Framework.DataValidator.Rules;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Ricebird.Framework.DataValidator
{
    public abstract class FluentValidator
    {
        public abstract ValidateResult Validate([NotNull] object validateObj, IClient client);
        public abstract object ToJsonObject();
        public abstract void AutoRulesByAttributes();
    }

    public class FluentValidator<T> : FluentValidator
        where T : new()
    {
        public Type ForType => typeof(T);

        protected List<ValidateRules<T>> RuleSets
        {
            get; set;
        } = [];

        internal ValidateRules<T>? FindRule(PropertyMeta meta)
        {
            return RuleSets.FirstOrDefault(e => e.Meta.Name == meta.Name);
        }

        #region 新建规则
        private ValidateRules<T> RuleFor(PropertyInfo member, string fieldName = "")
        {
            ParameterExpression paraE = Expression.Parameter(typeof(T), "e");
            Expression exp = Expression.MakeMemberAccess(paraE, member);
            Delegate func = Expression.Lambda(exp, paraE).Compile();

            fieldName = string.IsNullOrWhiteSpace(fieldName) ? member.Name : fieldName;
            PropertyMeta meta = new(member, func, typeof(T), fieldName);
            return RuleForInternal(meta);
        }

        public ValidateRules<T> RuleFor<TKey>(Expression<Func<T, TKey>> forKey)
        {
            return RuleFor(forKey, "");
        }

        public ValidateRules<T> RuleFor<TKey>(Expression<Func<T, TKey>> forKey, string fieldName)
        {
            PropertyMeta meta = PropertyMeta.GetProperyFromSelector(forKey, fieldName);
            return RuleForInternal(meta);
        }

        private ValidateRules<T> RuleForInternal(PropertyMeta meta)
        {
            ValidateRules<T>? rule = FindRule(meta);
            if (rule == null)
            {
                rule = new ValidateRules<T>(meta);
                RuleSets.Add(rule);
            }

            return rule;
        }
        #endregion

        #region 自动类型验证器
        /// <summary>
        /// 自动为项目类型添加验证
        /// </summary>
        /// <returns></returns>
        public override void AutoRulesByAttributes()
        {
            PropertyInfo[] props = typeof(T).GetProperties();
            Type nonAttr = typeof(NonValidationAttribute);

            Dictionary<Type, AbstactValidateRule<T>> ruleDict = new Dictionary<Type, AbstactValidateRule<T>>()
            {
                { typeof(int), new IsIntRule<T>() },
                { typeof(long), new IsLongRule<T>() },
                { typeof(float), new IsFloatRule<T>() },
                { typeof (double), new IsDoubleRule<T>() },
                { typeof(decimal), new IsDecimalRule<T>() },
                { typeof(Guid), new IsGuidRule<T>() },
                { typeof(DateTime), new IsDateRule<T>() },
            };

            foreach (var prop in props)
            {
                if (prop.IsDefined(nonAttr))
                {
                    continue;
                }

                DisplayNameAttribute? display = prop.GetCustomAttribute<DisplayNameAttribute>();
                string fieldName = display != null ? display.DisplayName : "";

                if (ruleDict.TryGetValue(prop.PropertyType, out var rule))
                {
                    RuleFor(prop, fieldName).AddRule(rule);
                }

                if (prop.IsDefined(typeof(MaxLengthAttribute)))
                {
                    MaxLengthAttribute? maxLengthAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
                    int maxLength = maxLengthAttr != null ? maxLengthAttr.Length : 0;
                    RuleFor(prop, fieldName).MaxLength(maxLength);
                }

                if (prop.IsDefined(typeof(StringLengthAttribute)))
                {
                    StringLengthAttribute? stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>();
                    int length = stringLengthAttribute != null ? stringLengthAttribute.MaximumLength : 0;
                    RuleFor(prop, fieldName).MaxLength(length);
                }

                if (prop.CustomAttributes.Any(e => e.AttributeType.Name.Contains("Required")))
                {
                    RuleFor(prop, fieldName).Required();
                }

                if (prop.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase) && prop.PropertyType == typeof(Guid))
                {
                    RuleFor(prop, fieldName).AddRule(new IdMustHaveValue<T>());
                }
            }
        }
        #endregion

        public override ValidateResult Validate([NotNull] object validateObj, IClient client)
        {
            ArgumentNullException.ThrowIfNull(validateObj);

            if (validateObj is not T valObj)
            {
                throw new InvalidCastException($"无法将参数{nameof(validateObj)}，由类型“{validateObj.GetType().FullName}”转换为类型“{typeof(T).FullName}”。");
            }

            ValidateResult result = new ValidateResult(validateObj);
            foreach (var rule in RuleSets)
            {
                rule.Validate(client, result, valObj);
            }

            return result;
        }

        public override object ToJsonObject()
        {
            Dictionary<string, object> result = [];

            foreach (var rule in RuleSets)
            {
                result.Add(rule.ForProperty, rule.ToJsonObject());
            }

            return result;
        }
    }
}
