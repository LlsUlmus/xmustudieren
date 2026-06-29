using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Ricebird.Framework.HtmlChecker.Nodes;
using Ricebird.Framework.HtmlChecker.Options;

namespace Ricebird.Framework.HtmlChecker
{
    public class HtmlChecker : ISingletonDependency
    {
        public FrozenDictionary<string, AvailableTag> TagAndAtrrs
        {
            get; init;
        }

        public IServiceProvider Services
        {
            get; set;
        }

        public HtmlChecker(IOptionService optService, IServiceProvider sp)
        {
            var opt = optService.LoadOptions<TagStrategyOption>();
            TagAndAtrrs = opt.AvailableTags.ToFrozenDictionary(e => e.TagName);
            Services = sp;
        }

        #region 解析标签
        public HtmlNode ParseNode(string strHtml) => ParseNode(strHtml, false);

        public HtmlNode ParseNode(string strHtml, bool removeHref)
        {
            try
            {
                var doc = new HtmlParser().ParseDocument(strHtml);
                StartNode startNode = Services.Resolve<StartNode>();
                ParseNodesInternal(doc.Body!.ChildNodes, startNode, removeHref, 1);
                return startNode;
            }
            catch
            {
                return Services.Resolve<StartNode>();
            }
        }

        private void ParseNodesInternal(INodeList nodes, HtmlNode parentNode, bool removeHref, int level)
        {
            foreach (INode node in nodes)
            {
                HtmlNode result;
                string nodeName = node.NodeName.ToLower().Trim();
                if (TagAndAtrrs.TryGetValue(nodeName, out var value))
                {
                    string finalTagName = value.ReplaceTo.HasValue() ? value.ReplaceTo : value.TagName;
                    // 如果是表里的
                    result = finalTagName switch
                    {
                        "img" => Services.Resolve<ImageNode>(),
                        "col" => Services.Resolve<ColNode>(),
                        "a" => Services.Resolve<AnchorNode>(),
                        _ => Services.Resolve<CommonNode>(),
                    };
                    result.IsSelfClose = value.IsSelfClose;
                    result.AvailableAttributes = value.Attributes;
                    result.IsBlockNode = value.IsBlockNode;
                    result.RemoveHref = removeHref;
                    result.TagName = finalTagName;
                }
                else if (nodeName == "#text")
                {
                    result = Services.Resolve<TextNode>();
                    result.InnerText = node.NodeValue;
                    result.TagName = nodeName;
                }
                else if (nodeName.Length == 0)
                {
                    continue;
                }
                else if (IsChineseCharacter(nodeName[0]))
                {
                    result = Services.Resolve<NotNode>();
                    result.TagName = nodeName;
                }
                else if (nodeName is "html" or "head" or "body")
                {
                    if (node.HasChildNodes)
                    {
                        ParseNodesInternal(node.ChildNodes, parentNode, removeHref, level);
                    }
                    continue;
                }
                else
                {
                    result = Services.Resolve<InvalidNode>();
                    result.TagName = nodeName;
                }

                result.Level = level;
                if (node is IHtmlElement htmlElement)
                {
                    ParseAttribute(result, htmlElement.Attributes);
                }

                parentNode.Children.Add(result);

                if (node.HasChildNodes)
                {
                    ParseNodesInternal(node.ChildNodes, result, removeHref, level + 1);
                }
            }
        }

        private static void ParseAttribute(HtmlNode node, INamedNodeMap attrs)
        {
            foreach (var item in attrs)
            {
                node.Attributes.MergeKey(item.Name, item.Value);
            }
        }

        private static bool IsChineseCharacter(char c)
        {
            int codePoint = c;
            // 汉字的Unicode编码范围是0x4E00到0x9FA5
            if (codePoint is >= 0x4E00 and <= 0x9FA5 or
                >= 0x9FA6 and <= 0x9FBF or // CJK Unified Ideographs Extension A
                >= 0xF900 and <= 0xFAFF or // CJK Compatibility Ideographs
                >= 0x20000 and <= 0x2A6DF)  // CJK Unified Ideographs Extension B
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
