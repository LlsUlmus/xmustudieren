
namespace Ricebird.Framework.HtmlChecker.Nodes
{
    public class ColNode : HtmlNode
    {
        protected override void RenderStartTag(StringBuilder builder)
        {
            builder.Append("<col ");
            RenderAttributes(builder);
            builder.Append('>');
        }

        protected override void RenderEndTag(StringBuilder builder)
        {

        }

        protected override void RenderContent(StringBuilder builder)
        {
        }
    }
}
