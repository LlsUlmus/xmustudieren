using Ricebird.Framework.DataValidator;

namespace Ricebird.Security.Models
{
    public class MenuItem : TreeEntityBase<MenuItem>, IValidatable
    {
        #region 数据库字段
        public LinkType LinkType
        {
            get; set;
        } = LinkType.Url;

        public Visibility Visibility
        {
            get; set;
        } = Visibility.Visible;

        public string Icon
        {
            get; set;
        } = string.Empty;

        public string LinkTo
        {
            get; set;
        } = string.Empty;

        public string QueryString
        {
            get; set;
        } = string.Empty;

        public string Parameters
        {
            get; set;
        } = string.Empty;
        #endregion

        public FluentValidator BuildValidator()
        {
            FluentValidator<MenuItem> fluent = new FluentValidator<MenuItem>();
            fluent.AutoRulesByAttributes();
            fluent.RuleFor(e => e.Name).Required("必须填写菜单名称");
            fluent.RuleFor(e => e.LinkTo).Required("必须填写链接地址");
            fluent.RuleFor(e => e.LinkType).Should((r, e) =>
            {
                if (e.ParentId != Guid.Empty && e.LinkType == LinkType.ParentNode)
                {
                    r.SetFailure(nameof(LinkType), "该菜单项存在上级菜单，所以不允许再配置上级菜单。");
                }

                if (e.LinkType == LinkType.ParentNode && e.LinkTo == "-")
                {
                    e.LinkTo = GenerateId(8);
                }
            });
            fluent.RuleFor(e => e.ParentId).ParentIdMustExists("找不到对应的父级菜单ID", true);
            fluent.RuleFor(e => e.ParentId).Should((r, e) =>
            {
                if (e.LinkType == LinkType.ParentNode)
                {
                    e.ParentId = Guid.Empty;
                }

                if (e.ParentId == e.ID)
                {
                    r.SetFailure(nameof(e.ParentId), "不能将自己设为自己的子页面");
                }
            });

            return fluent;
        }

        public object ToJsonObject() => new
        {
            display = Name,
            icon = Icon,
            manual = false,
            linkType = LinkType,
            path = LinkTo,
            name = ID.To62String(),
            order = DisplayOrder,
            children = Children.Select(e => e.ToJsonObject()).ToList(),
        };
    }
}
