namespace Ricebird.Framework.Configurations.OptionReaders
{
    internal class LocalOptionProvider : IOptionStore
    {
        public OptionSaveTo SaveTo => OptionSaveTo.FileSystem;

        public T LoadOptions<T>(T option)
            where T : IOption, new()
        {
            T opt = option;
            string file = $"./Configs/{opt.SaveKey}.json";
            EnsureDirectoryExists(file);

            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                var obj = DesearializeJson<T>(json);
                if (obj != null) return obj;
            }

            SaveOptions(opt);
            return opt;
        }

        public void SaveOptions(IOption opt)
        {
            string file = $"./Configs/{opt.SaveKey}.json";
            using Stream stream = new FileStream(file, FileMode.Create);
            string text = opt.SearializeJson(true);
            stream.WriteString(text);
        }
    }
}
