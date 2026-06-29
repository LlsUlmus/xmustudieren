using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json.Nodes;

namespace Ricebird.FileStorage.Services
{
    public class MimeTypeService : IMimeTypeService
    {
        protected HostEnv HostEnv { get; set; }
        public Dictionary<string, string> MimeTypes { get; set; } = [];
        public const string DisabledMimeType = "disabled";
        public const string DefaultMimeType = "application/octet-stream";

        public MimeTypeService(HostEnv env)
        {
            HostEnv = env;

            LoadMimeType();
        }

        internal void LoadMimeType()
        {
            string path = Path.Combine(HostEnv.AppRootPath, "Configs", "mime.json");
            try
            {
                string json = File.ReadAllText(path);
                var jsonNode = JsonNode.Parse(json);
                if (jsonNode == null)
                {
                    return;
                }

                JsonObject mimes = jsonNode.AsObject();
                var mimeDict = new Dictionary<string, string>();
                foreach (var mime in mimes)
                {
                    if (mime.Value != null)
                    {
                        string value = mime.Value.ToString();
                        mimeDict.MergeKey(mime.Key.ToLower(), value);
                    }
                }

                MimeTypes = mimeDict;
            }
            catch
            {

            }
        }

        internal void SaveMimeTypes()
        {
            string json = MimeTypes.SearializeJson();
            string path = Path.Combine(HostEnv.AppRootPath, "Configs", "mime.json");
            File.WriteAllText(path, json);
        }

        public void MergeMimeType(string key, string value)
        {
            MimeTypes.MergeKey(key, value);
            SaveMimeTypes();
        }

        public string GetMimeType(string extOrPath)
        {
            int extIndex = extOrPath.LastIndexOf('.');
            if (extIndex == -1)
            {
                return DisabledMimeType;
            }
            string ext = extOrPath[extIndex..].ToLower();
            if (MimeTypes.TryGetValue(ext, out string? value))
            {
                return value;
            }

            return DisabledMimeType;
        }

        public void RemoveMimeType(string key)
        {
            if (!MimeTypes.ContainsKey(key))
            {
                return;
            }
            MimeTypes.Remove(key);
            SaveMimeTypes();
        }

        public FileExtensionContentTypeProvider BuildContentTypeProvider()
        {
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
            foreach (var item in MimeTypes)
            {
                provider.Mappings.MergeKey(item.Key, item.Value);
            }
            HostEnv.WriteLog("文件服务", $"成功向服务器注入MIME TYPE {MimeTypes.Count}个。");
            return provider;
        }
    }
}
