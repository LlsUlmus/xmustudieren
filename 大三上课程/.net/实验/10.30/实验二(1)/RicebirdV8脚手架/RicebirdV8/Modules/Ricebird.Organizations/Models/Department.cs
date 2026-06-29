using Ricebird.Framework.DataValidator;
using System.ComponentModel.DataAnnotations;

namespace Ricebird.Organizations.Models
{
    public class Department : TreeEntityBase<Department>, IValidatable, IDepart
    {
        #region 数据库字段
        [StringLength(50)]
        public string Code
        {
            get; set;
        } = string.Empty;

        [StringLength(20)]
        public string ShortName
        {
            get; set;
        } = string.Empty;

        [StringLength(200)]
        public string Description
        {
            get; set;
        } = string.Empty;

        [StringLength(50)]
        public string Phone
        {
            get; set;
        } = string.Empty;

        [StringLength(50)]
        public string Fax
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 部门别名
        /// </summary>
        [StringLength(20)]
        public string SchemaName
        {
            get; set;
        } = "";

        /// <summary>
        /// 部门来源
        /// <para>
        /// 后台新建，后台导入，数据同步，系统生成
        /// </para>
        /// </summary>
        [StringLength(6)]
        public string Source
        {
            get; set;
        } = string.Empty;

        public bool IsDefault
        {
            get; set;
        } = false;

        /// <summary>
        /// 严格学分模式
        /// </summary>
        public bool StrictCreditStrategy
        {
            get; set;
        } = true;
        #endregion

        #region 非数据库字段
        [NotMapped]
        IEnumerable<IDepart> IDepart.AllChildren => AllChildren;
        #endregion

        public FluentValidator BuildValidator()
        {
            FluentValidator<Department> fluent = new FluentValidator<Department>();
            fluent.AutoRulesByAttributes();
            fluent.RuleFor(e => e.ParentId).ParentIdMustExists($"找不到{ParentId}对应的部门");
            return fluent;
        }

        public override Department CopyTo()
        {
            var dpt = base.CopyTo();
            dpt.Source = "系统生成";
            return dpt;
        }
    }
}
