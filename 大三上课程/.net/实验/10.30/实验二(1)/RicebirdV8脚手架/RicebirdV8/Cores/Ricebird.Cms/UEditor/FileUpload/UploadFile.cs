namespace UEditor.Services
{
    public class UploadFile : UploadHandler
    {
        public UploadFile(IClient workContext)
            : base(workContext)
        {
            UploadConfig = new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("fileAllowFiles"),
                PathFormat = Config.GetString("filePathFormat"),
                SizeLimit = Config.GetInt("fileMaxSize"),
                UploadFieldName = Config.GetString("fileFieldName")
            };
        }
    }
}