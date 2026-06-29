namespace UEditor.Services.ImageUpload
{
    public class UploadScrawl : UploadHandler
    {
        public UploadScrawl(IClient workContext) : base(workContext)
        {
            UploadConfig = new UploadConfig()
            {
                AllowExtensions = new string[] { ".png" },
                PathFormat = Config.GetString("scrawlPathFormat"),
                SizeLimit = Config.GetInt("scrawlMaxSize"),
                UploadFieldName = Config.GetString("scrawlFieldName"),
                Base64 = true,
                Base64Filename = "scrawl.png"
            };
        }
    }
}