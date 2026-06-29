namespace Ricebird.Framework.FileStorage
{
    public interface IFile
    {
        string PhysicPath
        {
            get; set;
        }

        string DownloadPath
        {
            get;
        }

        string UniqueCode
        {
            get;
        }

        string DisplayName
        {
            get;
        }

        FileStorageType StorageType { get; }
    }
}
