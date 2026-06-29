namespace Ricebird.Security.Models
{
    [DataDictionary("链接类型")]
    public enum LinkType
    {
        [DataEntry("普通URL")]
        Url = 1,
        [DataEntry("内置页面")]
        VuePage = 2,
        [DataEntry("上级菜单")]
        ParentNode = 3
    }

    public enum Visibility
    {
        Visible = 1,
        Hidden = 2,
    }
}
