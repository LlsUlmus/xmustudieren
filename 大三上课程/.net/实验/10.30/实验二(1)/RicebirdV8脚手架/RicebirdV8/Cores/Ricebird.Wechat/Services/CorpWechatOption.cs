namespace Ricebird.Wechat.Services
{
    public class CorpWechatOption : IOption
    {
        public string CorpId { get; set; } = "";

        public string CorpSecert { get; set; } = "";

        public string AgentId { get; set; } = "";

        public string Token { get; set; } = "";
        public string AesKey { get; set; } = "";

        public OptionSaveTo OptionSaveTo => OptionSaveTo.FileSystem;

        public string SaveKey => "corp-wechat";
    }
}
