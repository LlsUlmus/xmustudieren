namespace Ricebird.Framework.HtmlChecker.Nodes
{
    public class NotNode : HtmlNode
    {
        protected override void RenderStartTag(StringBuilder builder)
        {
            builder.Append("&lt;");
            builder.Append(TagName);
            builder.Append("&gt;");
        }

        protected override void RenderAttributes(StringBuilder builder)
        {
        }

        protected override void RenderEndTag(StringBuilder builder)
        {
        }

        protected override void GetInnerText(StringBuilder builder)
        {
            builder.Append($"<{TagName}>");
            GetInnerTextContent(builder);
        }
    }
}
