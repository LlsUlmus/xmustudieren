namespace Ricebird.Framework.HtmlChecker.Nodes
{
    public class TextNode : HtmlNode
    {
        protected override void RenderStartTag(StringBuilder builder)
        {
        }

        protected override void RenderAttributes(StringBuilder builder)
        {
        }

        protected override void RenderEndTag(StringBuilder builder)
        {
        }

        protected override void RenderContent(StringBuilder builder) => builder.Append(InnerText);

        protected override void GetInnerTextContent(StringBuilder builder) => builder.Append(InnerText);
    }
}
