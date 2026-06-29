namespace Ricebird.Wechat.Services
{
    public class WechatOption : IOption
    {
        public string AppId { get; set; } = "";

        public string AppSecert { get; set; } = "";

        public OptionSaveTo OptionSaveTo => OptionSaveTo.FileSystem;

        public string SaveKey => "wechat";
    }
}
