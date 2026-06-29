namespace Ricebird.FileStorage.Models
{
    public class TemporaryFile : IFile
    {
        public Guid ID
        {
            get; set;
        } = Guid.NewGuid();

        public string PhysicPath
        {
            get; set;
        } = string.Empty;

        public string DisplayName
        {
            get; set;
        } = string.Empty;

        public string DownloadPath
        {
            get
            {
                var fileName = Path.GetFileName(PhysicPath);

                string[] array = fileName.Split('+');
                string actualFileName = $"{array[1]}.{array[0]}";
                return $"/tempory/{actualFileName}";
            }
        }

        public string UniqueCode => ID.To62String();
        public FileStorageType StorageType => FileStorageType.Temporary;
    }
}
