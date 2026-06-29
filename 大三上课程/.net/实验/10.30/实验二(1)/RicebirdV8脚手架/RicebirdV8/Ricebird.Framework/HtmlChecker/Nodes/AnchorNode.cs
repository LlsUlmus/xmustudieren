namespace Ricebird.Framework.HtmlChecker.Nodes
{
    public class AnchorNode : HtmlNode
    {
        protected override (string key, string value) FormatAttribute(string key, string value)
        {
            if (key == "href" && RemoveHref)
            {
                // 特别处理
                return (key, "javascript:void(0);");
            }

            return base.FormatAttribute(key, value);
        }
    }
}
