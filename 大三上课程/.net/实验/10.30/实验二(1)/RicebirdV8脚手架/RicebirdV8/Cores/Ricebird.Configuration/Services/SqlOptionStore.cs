namespace Ricebird.Configuration.Services
{
    internal class SqlOptionStore(ConfigRepository rep) : IOptionStore
    {
        public ConfigRepository rep = rep;

        public OptionSaveTo SaveTo => OptionSaveTo.Database;

        public T LoadOptions<T>(T option) where T : IOption, new()
        {
            var config = rep.FirstOrNew(e => e.Key == option.SaveKey) ?? new Models.Configuration();
            T? opt = DesearializeJson<T>(config.Value);

            if (opt is null)
            {
                config.Key = option.SaveKey;
                config.Value = option.SearializeJson();
                rep.Save(config);
                opt = option;
            }
            else
            {
                option = opt;
            }

            return opt;
        }

        public void SaveOptions(IOption opt)
        {
            var config = rep.FirstOrNew(e => e.Key == opt.SaveKey) ?? new Models.Configuration();
            config.Key = opt.SaveKey;
            config.Value = opt.SearializeJson();
            rep.Save(config);
        }
    }
}
