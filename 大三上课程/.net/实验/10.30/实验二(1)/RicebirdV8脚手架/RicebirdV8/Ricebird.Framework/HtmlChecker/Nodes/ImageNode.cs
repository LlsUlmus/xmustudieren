namespace Ricebird.Framework.HtmlChecker.Nodes
{
    // public class ImageNode(IFileStorageService fileStorage) : HtmlNode
    public class ImageNode : HtmlNode
    {
        protected override (string key, string value) FormatAttribute(string key, string value)
        {
            string srcValue = value;
            if (key == "src")
            {
                // 特别处理
                return (key, srcValue);
            }

            return base.FormatAttribute(key, value);
        }
    }
}
