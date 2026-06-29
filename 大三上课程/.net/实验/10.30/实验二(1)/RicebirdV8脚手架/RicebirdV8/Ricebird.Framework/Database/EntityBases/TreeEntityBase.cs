using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Framework.Database
{
    public abstract class TreeEntityBase<TNode> : EntityBase
        where TNode : TreeEntityBase<TNode>, new()
    {
        #region 数据库字段
        /// <summary>
        /// 节点名称
        /// </summary>
        [Required]
        public virtual string Name
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 降序排序字段
        /// </summary>
        public virtual int DisplayOrder
        {
            get; set;
        } = 0;

        /// <summary>
        /// 母元素
        /// </summary>
        public Guid ParentId
        {
            get; set;
        } = Guid.Empty;

        public virtual string InternalTreeCode
        {
            get; set;
        } = string.Empty;
        #endregion

        #region 非数据库字段
        [NotMapped]
        [JsonIgnore]
        public TNode? Parent
        {
            get; set;
        }

        protected List<TNode> _children = [];
        /// <summary>
        /// 取得所有子元素
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public IEnumerable<TNode> Children
        {
            get
            {
                _children ??= [];

                return _children;
            }
            set => _children = value.ToList();
        }

        public void AddChild(TNode child) => _children?.Add(child);

        protected List<TNode> _allChildren = [];
        /// <summary>
        /// 取得所有子元素
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public IEnumerable<TNode> AllChildren
        {
            get
            {
                _allChildren ??= [];

                return _allChildren;
            }
            set => _allChildren = value.ToList();
        }

        public void AddAllChildren(TNode child) => _allChildren?.Add(child);

        public virtual TNode CopyTo()
        {
            TNode dest = new TNode();
            dest.CopyPropertiesFrom(this);
            dest.ID = SequentialGuid.NewSuid();
            dest.DisplayOrder = DisplayOrder;
            return dest;
        }
        #endregion

        public override string ToString() => ToString(" >> ");

        public virtual string ToString(string seperator)
        {
            string result = Name;
            for (TNode? d = Parent; d != null; d = d.Parent)
            {
                result = d.Name + seperator + result;
            }

            return result;
        }
    }
}
