namespace Ricebird.Framework.Organizations
{
    public interface IDepart
    {
        Guid ID
        {
            get; set;
        }

        /// <summary>
        /// 节点名称
        /// </summary>
        string Name
        {
            get; set;
        }

        /// <summary>
        /// 部门简称
        /// </summary>
        string ShortName
        {
            get; set;
        }

        /// <summary>
        /// 降序排序字段
        /// </summary>
        int DisplayOrder
        {
            get; set;
        }

        /// <summary>
        /// 母元素
        /// </summary>
        Guid ParentId
        {
            get; set;
        }

        string SchemaName { get; set; }
        string Code { get; set; }
        string Source { get; set; }
        string Description { get; set; }
        string Fax { get; set; }
        string InternalTreeCode { get; set; }
        string Phone { get; set; }

        public IEnumerable<IDepart> AllChildren { get; }
    }
}