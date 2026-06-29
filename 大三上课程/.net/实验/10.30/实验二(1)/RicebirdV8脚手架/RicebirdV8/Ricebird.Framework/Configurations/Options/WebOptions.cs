namespace Ricebird.Framework.Configurations
{
    public class WebOptions : IOption
    {
        public OptionSaveTo OptionSaveTo => OptionSaveTo.Database;

        public string SaveKey => "WebOptions";

        public bool Debug
        {
            get; set;
        } = true;

        /// <summary>
        /// 网站名
        /// </summary>
        public string WebsiteName
        {
            get; set;
        } = "米雀框架";

        /// <summary>
        /// 网站的图标
        /// </summary>
        public string WebsiteAvatar
        {
            get; set;
        } = string.Empty;

        public string AuthenKey
        {
            get; set;
        } = ConstKeys.AuthenticationKey;

        public string Entry
        {
            get; set;
        } = "";

        public string Token
        {
            get; set;
        } = "";
    }
}
