using System.Web;

namespace Ricebird.Framework.HtmlChecker.Nodes
{
    public abstract class HtmlNode : IDependency
    {
        #region 属性
        /// <summary>
        /// 标签名
        /// </summary>
        public string TagName
        {
            get;
            set;
        } = string.Empty;

        public string ReplaceTo
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 是否自闭
        /// </summary>
        public bool IsSelfClose
        {
            get;
            set;
        } = false;

        public bool IsBlockNode
        {
            get; set;
        } = false;

        public bool RemoveHref
        {
            get; set;
        } = false;

        public string[] AvailableAttributes
        {
            get;
            set;
        } = [];

        public Dictionary<string, string> Attributes
        {
            get; set;
        } = [];

        public List<HtmlNode> Children
        {
            get; set;
        } = [];

        public string InnerText
        {
            get; set;
        } = string.Empty;

        public int Level
        {
            get; set;
        } = 1;
        #endregion

        public override string ToString() => $"<{TagName}>";

        #region 渲染函数
        public string RenderNodes() => RenderNodes(new StringBuilder()).ToString();

        protected StringBuilder RenderNodes(StringBuilder builder)
        {
            RenderStartTag(builder);
            RenderContent(builder);
            RenderEndTag(builder);

            return builder;
        }

        protected virtual void RenderStartTag(StringBuilder builder)
        {
            builder.Append("<");
            builder.Append(TagName);
            RenderAttributes(builder);
            if (IsSelfClose)
            {
                builder.Append("/>");
            }
            else
            {
                builder.Append('>');
            }
        }

        protected virtual void RenderAttributes(StringBuilder builder)
        {
            List<string> attrs = [];
            foreach (var item in Attributes)
            {
                if (AvailableAttributes.Contains(item.Key))
                {
                    var (k, v) = FormatAttribute(item.Key, item.Value);
                    char quotation = v.Contains('"') ? '\'' : '"';
                    attrs.Add($"{k}={quotation}{v}{quotation}");
                }
            }
            string str = attrs.JoinAsString(' ');
            if (str.Length > 0)
            {
                builder.Append(' ');
                builder.Append(str);
            }
        }

        protected virtual (string key, string value) FormatAttribute(string key, string value) => (key, value);

        protected virtual void RenderEndTag(StringBuilder builder)
        {
            if (!IsSelfClose)
            {
                builder.Append($"</{TagName}>");
            }
        }

        protected virtual void RenderContent(StringBuilder builder)
        {
            foreach (var node in Children)
            {
                node.RenderNodes(builder);
            }
        }
        #endregion

        #region 只获取里面的文字
        public string GetInnerText()
        {
            StringBuilder builder = new StringBuilder();
            GetInnerText(builder);
            string str = HttpUtility.HtmlDecode(builder.ToString());

            return str;
        }

        protected virtual void GetInnerText(StringBuilder builder)
        {
            GetInnerTextContent(builder);
            if (IsBlockNode)
            {
                builder.Append("\r\n");
            }
        }

        protected virtual void GetInnerTextContent(StringBuilder builder)
        {
            foreach (var node in Children)
            {
                node.GetInnerText(builder);
            }
        }
        #endregion
    }
}
