namespace Ricebird.Framework.FileStorage
{
    public class FileStorageOption : IOption
    {
        [JsonIgnore]
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public HostEnv Env { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public OptionSaveTo OptionSaveTo => OptionSaveTo.Database;
        public string SaveKey => nameof(FileStorageOption);

        public string StorageDirectory
        {
            get;
            set;
        } = "Storage";

        public string PermanentDirectory
        {
            get;
            set;
        } = "Permanent";

        public string TemporaryDirectory
        {
            get; set;
        } = "Temporary";

        /// <summary>
        /// 上传文件大小限制，单位B
        /// </summary>
        public int MaxSizeLimit
        {
            get; set;
        } = 31_457_280;

        public string GetPhyicPermanentDirectory()
        {
            HostEnv env = HostEnv.Instance;
            var dir = Path.Combine(env.AppRootPath, StorageDirectory, PermanentDirectory);
            return dir;
        }

        public string GetPhyicTemporaryDirectory()
        {
            HostEnv env = HostEnv.Instance;
            var dir = Path.Combine(env.AppRootPath, StorageDirectory, TemporaryDirectory);
            return dir;
        }
    }

}
