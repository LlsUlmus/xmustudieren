namespace Ricebird.Framework.Database
{
    public abstract class OrderedEntityBase : EntityBase
    {
        public int DisplayOrder
        {
            get; set;
        } = 0;
    }

    public abstract class DescendingEntityBase : OrderedEntityBase, IComparable<OrderedEntityBase>
    {
        // 返回 -1 排序是 [this, other]
        // 返回 0 说明是相同
        // 返回 1 排序是 [other, this]

        public virtual int CompareTo(OrderedEntityBase? other) => other == null ? 1 : DisplayOrder < other.DisplayOrder ? 1 : -1;
    }

    public abstract class AscendingEntityBase : OrderedEntityBase, IComparable<OrderedEntityBase>
    {
        public virtual int CompareTo(OrderedEntityBase? other) => other == null ? 1 : DisplayOrder > other.DisplayOrder ? 1 : -1;
    }
}
