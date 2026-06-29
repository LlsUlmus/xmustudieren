namespace UEditor.Services
{
    public class UploadImage : UploadHandler
    {
        public UploadImage(IClient context)
            : base(context)
        {
            UploadConfig = new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("imageAllowFiles"),
                PathFormat = Config.GetString("imagePathFormat"),
                SizeLimit = Config.GetInt("imageMaxSize"),
                UploadFieldName = Config.GetString("imageFieldName")
            };
        }
    }
}
