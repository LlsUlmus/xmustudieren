namespace UEditor.Services
{
    public class UploadVideo : UploadHandler
    {
        public UploadVideo(IClient workContext) : base(workContext)
        {
            UploadConfig = new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("videoAllowFiles"),
                PathFormat = Config.GetString("videoPathFormat"),
                SizeLimit = Config.GetInt("videoMaxSize"),
                UploadFieldName = Config.GetString("videoFieldName")
            };
        }
    }
}