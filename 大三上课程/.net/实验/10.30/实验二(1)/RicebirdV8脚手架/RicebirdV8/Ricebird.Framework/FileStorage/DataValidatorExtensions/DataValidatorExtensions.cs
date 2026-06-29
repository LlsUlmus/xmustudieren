#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using Ricebird.Framework.Clients;
using Ricebird.Framework.FileStorage.DataValidatorExtensions;

namespace Ricebird.Framework.DataValidator
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
{
    public static class DataValidatorExtensions
    {
        /// <summary>
        /// 验证附件必须存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static ValidateRules<T> AttachmentMustExits<T>(this ValidateRules<T> rules, string usage)
        {
            rules.AddRule(new AttachmentMustExistsRule<T>(usage, o => true));
            return rules;
        }

        /// <summary>
        /// 根据条件验证附件必须存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rules"></param>
        /// <param name="usage"></param>
        /// <param name="condition">仅当条件为真时，才进行验证</param>
        /// <returns></returns>
        public static ValidateRules<T> AttachmentMustExits<T>(this ValidateRules<T> rules, string usage, Predicate<IClient> condition)
        {
            rules.AddRule(new AttachmentMustExistsRule<T>(usage, condition));
            return rules;
        }
    }
}
