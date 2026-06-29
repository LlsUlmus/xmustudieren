using Ricebird.Framework.Clients;

namespace Ricebird.Framework.FileStorage
{
    public interface IFileStorageService : IScopedDependency
    {
        (string msg, IFile? file) CreateFile(byte[] bytes, string srcFileName, string module, IClient client);
        (string msg, IFile? file) CreateFile(Stream stream, string srcFileName, string module, IClient client);
        (string msg, IFile? file) CreateFile(string base64Str, string srcFileName, string module, IClient client);
        (string msg, IFile? file) CreateTemporaryFile(Stream stream, string srcFileName);
        (string msg, IFile? file) CreateTemporaryFile(byte[] bytes, string srcFileName);
        (FileStream stream, IFile file) CreateTemporaryFile(string srcFileName);
        void DeleteTemporaryFile(IFile file);
        void DeleteFile(Guid code);
        IFile? GetFile(Guid id);

        bool IsFileInStorage(PathString pathString);
        (byte[]? bytes, string mimeType, string downloadFileName, string displayName) GetFileBytes(PathString pathString);
        (byte[]? bytes, string mimeType, string downloadFileName, string displayName) GetFinalFile(string path, string displayName = "");
    }
}
