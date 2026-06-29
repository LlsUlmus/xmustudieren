using Ricebird.Cms.Models;
using Ricebird.Cms.Models.Validators;

namespace Ricebird.Framework.DataValidator
{
    internal static class ValidationExtensions
    {
        public static ValidateRules<T> CategoryMustExist<T>(this ValidateRules<T> rules, bool allowGuidEmpty)
        {
            rules.AddRule(new CategoryMustExistsRule<T>(allowGuidEmpty));
            return rules;
        }

        [Obsolete("该方法已经废弃")]
        public static ValidateRules<Category> UniqueCodeValidation(this ValidateRules<Category> rules)
        {
            rules.AddRule(new CodeMustUniqueRule());
            return rules;
        }

        public static ValidateRules<Category> HomeMustUnique(this ValidateRules<Category> rules)
        {
            rules.AddRule(new HomeMustUniqueRule());
            return rules;
        }

        public static ValidateRules<Category> HomeMustAtRoot(this ValidateRules<Category> rules)
        {
            rules.AddRule(new HomeMustAtRootRule());
            return rules;
        }

        public static ValidateRules<Category> ParentCategoryLimit(this ValidateRules<Category> rules)
        {
            rules.AddRule(new ParentCategoryLimitRule());
            return rules;
        }
    }
}
