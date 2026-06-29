using Ricebird.Framework.Clients;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Framework.DataValidator
{
    public class ValidateRules<T>
    {
        internal PropertyMeta Meta { get; set; }
        public Type ForType => Meta.ForType;
        public string ForProperty => Meta.Name;

        public List<AbstactValidateRule<T>> Rules
        {
            get;
            set;
        } = [];

        internal ValidateRules(PropertyMeta meta)
        {
            Meta = meta;
        }

        public AbstactValidateRule<T> AddRule(AbstactValidateRule<T> rule)
        {
            var exists = Rules.FirstOrDefault(e => e.RuleName == rule.RuleName);
            if (!rule.Multiple && exists != null)
            {
                Rules.Remove(exists);
                // throw new RuleAlreadyExistsException(ForType, rule.RuleName);
            }

            Rules.Add(rule);
            return rule;
        }

        public ValidateRules<T> AddRule<TValidator>(Func<ValidateRules<T>, TValidator> validatorBuilder)
            where TValidator : AbstactValidateRule<T>
        {
            TValidator vaildator = validatorBuilder(this);
            AddRule(vaildator);
            return this;
        }

        #region 验证器
        /// <summary>
        /// 必填验证器，默认的提示词是：必须填写{0}
        /// </summary>
        /// <returns></returns>
        public ValidateRules<T> Required()
        {
            AddRule(vr => new RequiredRule<T>());
            return this;
        }

        /// <summary>
        /// 必填验证器，默认的提示词是：必须填写{0}
        /// </summary>
        /// <param name="message">{0} 字段名</param>
        /// <returns></returns>
        public ValidateRules<T> Required(string message)
        {
            AddRule(vr => new RequiredRule<T>(message));
            return this;
        }
        #endregion

        /// <summary>
        /// 实际验证
        /// </summary>
        /// <param name="validateObject">必须不为NULL</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ValidateResult Validate(IClient client, ValidateResult result, T validateObject)
        {
            if (validateObject == null)
            {
                throw new ArgumentNullException(nameof(validateObject));
            }

            string fieldName = Meta.FieldName;
            object? fieldValue = Meta.GetData(validateObject);

            foreach (var rule in Rules)
            {
                result.CurrentProperty = Meta.Name;
                rule.Validate(client, result, validateObject, Meta.Name, fieldName, fieldValue);
            }

            return result;
        }

        public IEnumerable<object> ToJsonObject()
        {
            foreach (var rule in Rules)
            {
                yield return rule.ToJsonObject(Meta.FieldName);
            }
        }
    }
}
