using Microsoft.AspNetCore.StaticFiles;

namespace Ricebird.Framework.FileStorage
{
    public interface IMimeTypeService : ISingletonDependency
    {
        Dictionary<string, string> MimeTypes { get; set; }

        FileExtensionContentTypeProvider BuildContentTypeProvider();
        string GetMimeType(string extOrPath);
        void MergeMimeType(string key, string value);
        void RemoveMimeType(string key);
    }
}
