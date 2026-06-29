using Ricebird.Framework.Clients;
using Ricebird.Framework.Database;

namespace Ricebird.Framework.DataValidator.Rules.TreeRules
{
    internal class ParentIdMustExistsRule<T> : AbstactValidateRule<T>
        where T : TreeEntityBase<T>, new()
    {
        public override bool Multiple => false;

        public override string RuleName => "母节点必须存在";

        public bool AllowGuidEmpty
        {
            get; init;
        }

        /// <summary>
        /// message格式化的第一个参数是value, 第二个参数是字段名
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowGuidEmpty"></param>
        internal ParentIdMustExistsRule(string message, bool allowGuidEmpty)
        {
            Message = message;
            AllowGuidEmpty = allowGuidEmpty;
        }

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            TreeRepositoryBase<T> repository = client.Resolve<TreeRepositoryBase<T>>();
            if (validateObj.ParentId == Guid.Empty)
            {
                return;
            }

            T? node = repository.DbSet.FirstOrDefault(e => e.ID == validateObj.ParentId);
            if (node == null)
            {
                result.SetFailure(propertyName, Message.FormatString(validateObj.ParentId, fieldName));
            }
            else
            {
                validateObj.Parent ??= node;
            }
        }
    }
}
