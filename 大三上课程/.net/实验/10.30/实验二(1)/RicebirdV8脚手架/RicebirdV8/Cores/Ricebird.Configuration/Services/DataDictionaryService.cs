using System.Reflection;

namespace Ricebird.Configuration.Services
{
    public class DataDictionaryService(IServiceProvider provider) : IDataDictionaryService
    {
        #region ctor
        private HostEnv HostEnv => provider.Resolve<HostEnv>();
        private List<DataDictionary> _dictionary = [];
        private string _dictJson = "";
        public List<DataDictionary> DataDictionaries
        {
            get
            {
                if (_dictionary is null || _dictionary.Count == 0)
                {
                    _dictionary = LoadDictionary(provider);
                }
                return _dictionary;
            }
        }

        internal string Json
        {
            get
            {
                if (_dictJson.IsNullOrWhiteSpace())
                {
                    var obj = new
                    {
                        success = true,
                        msg = "",
                        data = DataDictionaries
                    };
                    _dictJson = obj.SearializeJson();
                }

                return _dictJson;
            }
        }

        public List<DataDictionary> LoadDictionary(IServiceProvider? pro)
        {
            List<DataDictionary> result = [];
            result.AddRange(LoadInternalEnum());
            using (var scope = (pro ?? provider).CreateScope())
            {
                IClient client = scope.CreateClient(MODULE_NAME);
                result.AddRange(LoadDatabaseItem(client));
            }
            result.Sort();
            _dictionary.Clear();
            _dictJson = string.Empty;
            return result;
        }

        private List<DataDictionary> LoadInternalEnum()
        {
            List<DataDictionary> result = [];
            // 读取内置的数据字典
            var dicts = HostEnv.DataDictionaries;
            int i = 10;
            foreach (Type dicType in dicts)
            {
                string name = dicType.GetCustomAttribute<DataDictionaryAttribute>()?.Name ?? dicType.Name;

                FieldInfo[] fieldInfos = dicType.GetFields();
                DataDictionary dict = new DataDictionary(name, DictionaryFrom.Enum, i);
                i += 10;

                int j = 0;
                foreach (var fi in fieldInfos)
                {
                    DataEntryAttribute? dea = fi.GetCustomAttribute<DataEntryAttribute>();
                    if (dea == null)
                    {
                        continue;
                    }

                    object enumValue = Enum.Parse(dicType, fi.Name);
                    object keyValue;
                    try
                    {
                        keyValue = Convert.ToInt32(enumValue);
                    }
                    catch
                    {
                        keyValue = enumValue;
                    }
                    string? key = !string.IsNullOrWhiteSpace(dea.Key) ? dea.Key : keyValue.ToString();
                    string value = dea.Value;
                    int order = dea.Order >= 0 ? dea.Order : j;
                    key = string.IsNullOrWhiteSpace(key) ? value : key;
                    dict.CreateEntry(key, value, order, dea.Enable, dea.Visible);
                    j += 10;
                }

                if (result.Any(e => e.Name == name)) throw new InvalidDataException($"已经存在一个名为{name}的字典");
                result.Add(dict);
            }
            return result;
        }

        private static List<DataDictionary> LoadDatabaseItem(IClient client)
        {
            DictionaryRepository repo = client.Resolve<DictionaryRepository>();
            var dict = repo.DbSet.OrderBy(e => e.DisplayOrder).Include(e => e.Entries.OrderBy(e => e.DisplayOrder)).ToList();
            foreach (var item in dict)
            {
                if (item.From == DictionaryFrom.Enum)
                {
                    item.From = DictionaryFrom.Database; // 数据库里不存在枚举类型
                }

                // item.Entries.Sort();
            }
            repo.SaveChanges();
            return dict;
        }
        #endregion

        #region Ensure函数
        public DataDictionary EnsureCreate(IClient client, string dictName, Action<DataDictionary> entryBuilder)
        {
            HostEnv host = client.Resolve<HostEnv>();
            DictionaryRepository repo = client.Resolve<DictionaryRepository>();
            DataDictionary? dict = this[dictName];
            bool addFlag = false;
            if (dict == null)
            {
                host.WriteLog(MODULE_NAME, $"检测到未生成的字典{dictName}，正在生成中");
                dict = new DataDictionary(dictName, DictionaryFrom.EnsureFunction, 100);
                repo.DbSet.Add(dict);
                addFlag = true;
            }

            dict.CanDelete = false;
            dict.CanEdit = true;
            if (dict.From == DictionaryFrom.Enum)
            {
                return dict;
            }

            var entryContainer = new DataDictionary()
            {
                ID = dict.ID,
                Name = dictName,
                Entries = []
            };
            entryBuilder(entryContainer);

            foreach (var entry in entryContainer.Entries)
            {
                entry.CanDelete = false;
                entry.CanEdit = false;
                if (dict[entry.DataKey] is null)
                {
                    host.WriteLog(MODULE_NAME, $"检测到未生成的字典项{entry}，正在生成中");
                    repo.DictionaryEntries.Add(entry);
                }
            }

            repo.SaveChanges();

            if (addFlag)
            {
                _dictionary.Add(dict);
            }

            return dict;
        }
        #endregion

        public DataDictionary? this[Guid id] => DataDictionaries.FirstOrDefault(e => e.ID == id);

        public DataDictionary? this[string name] => DataDictionaries.FirstOrDefault(e => e.Name == name);

        public Dictionary<string, string>? GetDictionary(string name)
        {
            var dict = this[name];
            if (dict == null) return null;

            return dict.ToDictionary();
        }

        public Dictionary<string, string> GetRequiredDictionary(string name)
        {
            var dict = this[name] ?? throw new InvalidOperationException($"找不到名为{name}的字典");
            return dict.ToDictionary();
        }

        #region 字典添加和删除操作
        public (bool success, string msg, ValidateResult result, DataDictionary? entity) SaveDictionary(IClient client)
        {
            DictionaryRepository repo = client.Resolve<DictionaryRepository>();
            client.MergeData(nameof(DataDictionary.From), DictionaryFrom.Database);
            var (opera, entity) = repo.FillEntity(client);
            var result = entity.Validate(client);

            if (!result)
            {
                return (false, "", result, null);
            }

            repo.SaveChanges();

            if (opera == DbOperate.Create)
            {
                _dictionary.Add(entity);
                _dictionary.Sort();
            }
            else
            {
                var exist = _dictionary.First(e => e.ID == entity.ID);
                exist.Name = entity.Name;
            }
            _dictJson = string.Empty;

            return (true, "", result, entity);
        }

        public DataDictionary? RemoveDictionary(IClient client)
        {
            Guid id = client.Get(nameof(id), Guid.Empty);

            var dict = this[id];
            if (dict is not null && dict.CanDelete)
            {
                DictionaryRepository repo = client.Resolve<DictionaryRepository>();
                repo.DbSet.Where(e => e.ID == dict.ID).ExecuteDelete();
                _dictionary.Remove(dict);
                _dictJson = string.Empty;
                return dict;
            }

            return null;
        }
        #endregion

        #region 字典项的添加和删除操作
        public (bool success, string msg, ValidateResult result, DictionaryEntry? entity) SaveDictionaryEntry(IClient client)
        {
            EntryRepository repo = client.Resolve<EntryRepository>();
            client.MergeData(nameof(DataDictionary.From), DictionaryFrom.Database);
            var (_, entity) = repo.FillEntity(client);
            var result = entity.Validate(client);
            if (!result)
            {
                return (false, "", result, null);
            }

            DataDictionary dict = this[entity.DataDictionaryId]!;
            dict.CreateEntry(entity);
            repo.SaveChanges();

            _dictJson = string.Empty;

            return (true, "", result, entity);
        }

        public DictionaryEntry? RemoveDictionaryEntry(IClient client)
        {
            Guid id = client.Get(nameof(id), Guid.Empty);

            EntryRepository repo = client.Resolve<EntryRepository>();
            DictionaryEntry? entity = repo.FirstOrDefault(e => e.ID == id);
            if (entity != null)
            {
                DataDictionary dict = this[entity.DataDictionaryId]!;
                if (dict.CanEdit && entity.CanDelete)
                {
                    repo.Remove(entity);
                    repo.SaveChanges();
                    dict.RemoveEntry(entity);
                    _dictJson = string.Empty;
                    return entity;
                }
            }

            return null;
        }

        public void ReorderEntry(IClient client)
        {
            Guid id = client.Get(nameof(id), Guid.Empty);

            EntryRepository repo = client.Resolve<EntryRepository>();
            DataDictionary? entity = this[id];
            if (entity == null) return;

            int i = 100;
            foreach (var item in entity.Entries.OrderBy(e => e.DisplayOrder))
            {
                item.DisplayOrder = i;
                repo.DbSet.Where(e => e.ID == item.ID).ExecuteUpdate(set => set.SetProperty(e => e.DisplayOrder, i));

                i += 100;
            }

            repo.SaveChanges();
            entity._dictionary.Clear();
            _dictJson = string.Empty;
        }
        #endregion
    }
}
