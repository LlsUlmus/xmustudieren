namespace Ricebird.Framework.HtmlChecker.Options
{
    public class TagStrategyOption : IOption
    {
        public AvailableTag[] AvailableTags { get; set; } = [];

        public OptionSaveTo OptionSaveTo => OptionSaveTo.FileSystem;

        public string SaveKey => "TagStrategy";
    }

    public record AvailableTag(string TagName, string ReplaceTo, string[] Attributes, string ClassName = "CommonNode", bool IsSelfClose = false, bool IsBlockNode = false);
}
