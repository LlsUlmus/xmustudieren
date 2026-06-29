using Ricebird.Framework.Clients;
using Ricebird.Framework.Database;

namespace Ricebird.Framework.Security.Apis
{
    public class ApiManager : ISingletonDependency
    {
        private HostEnv HostEnv { get; set; }

        private FrozenDictionary<string, ApiDescriptor> _descriptors;
        public FrozenDictionary<string, ApiDescriptor> Descriptors
        {
            get => _descriptors;
            set
            {
                _descriptors = value;
                TryUpdateJson();
            }
        }

        private FrozenDictionary<string, ApiDescriptor> _connectings;
        public FrozenDictionary<string, ApiDescriptor> Connectings
        {
            get => _connectings;
            set
            {
                _connectings = value;
                TryUpdateJson();
            }
        }

        private Stopwatch Watch { get; set; }
        private const string file = $"./Configs/apis.json";
        public string Json => ToJson();

        public ApiManager(HostEnv env)
        {
            HostEnv = env;
            Watch = Stopwatch.StartNew();
            List<ApiDescriptor> descriptors = LoadApis();

            foreach (WebModule module in env.AllWebModules)
            {
                int order = module.Priority * 1000;
                var list = module.Apis
                    .Select(e =>
                    {
                        e.DisplayOrder = order;
                        order += 1;
                        return e;
                    })
                    .OrderBy(e => e.ApiType)
                    .ThenBy(e => e.ApiGroup)
                    .ThenBy(e => e.DisplayOrder)
                    .ToList();
                MergeDescriptor(descriptors, list);
            }
            descriptors.Sort();
            _descriptors = descriptors.ToFrozenDictionary(e => e.Name);
            BuildFinalLinkTo(_descriptors);
            _connectings = descriptors.Where(e => e.Status == ApiStatus.Connecting).ToFrozenDictionary(e => e.Name);
            SaveApis();
        }

        private static void BuildFinalLinkTo(FrozenDictionary<string, ApiDescriptor> descriptors)
        {
            foreach (var item in descriptors)
            {
                if (item.Value.LinkTo.HasValue())
                {
                    ApiDescriptor? parentItem = item.Value;
                    do
                    {
                        if (parentItem.LinkTo.IsNullOrWhiteSpace()) break; // 找非空的就好
                        item.Value.FinalLinkTo = parentItem.LinkTo;
                    } while (descriptors.TryGetValue(item.Value.FinalLinkTo, out parentItem));
                }
            }
        }

        public string ToJson()
        {
            var query = Descriptors.Select(e => e.Value).OrderBy(e => e.DisplayOrder).GroupBy(e => e.Module);

            List<object> data = [];
            foreach (var item in query)
            {
                ApiDescriptor module = this[item.Key]!;
                var children = new List<object>();
                ApiCounter Success = new(), Failure = new(), Exceptions = new();
                foreach (var api in item)
                {
                    if (api.ApiType == ApiType.Module) continue;
                    children.Add(api);
                    Success += api.Success;
                    Failure += api.Failure;
                    Exceptions += api.Exceptions;
                }

                object obj = new
                {
                    Name = module?.Name ?? item.Key,
                    ApiType = ApiType.Module,
                    Status = ApiStatus.None,
                    DisplayOrder = module?.DisplayOrder ?? 0,
                    Module = module?.Module ?? item.Key,
                    ApiGroup = module?.ApiGroup ?? "",
                    AuthorizeLevel = -1,
                    Success,
                    Failure,
                    Exceptions,
                    children
                };
                data.Add(obj);
            }

            return (new
            {
                success = true,
                msg = "",
                data
            }).SearializeJson();
        }

        public ApiDescriptor? this[string name]
        {
            get
            {
                TryUpdateJson();
                return Descriptors.TryGetValue(name, out var result) ? result : null;
            }
        }

        public void Log(string name, ApiResult result, int total, IClient client)
        {
            RicebirdContext ctx = client.Resolve<RicebirdContext>();
            DatabaseDiagnostic sql = ctx.DbDiagnostic;

            ApiDescriptor? api = this[name];
            if (api == null)
            {
                return;
            }

            switch (result)
            {
                case ApiResult.Success:
                    api.Success += (total, sql.SqlCount);
                    break;
                case ApiResult.Failure:
                    api.Failure += (total, sql.SqlCount);
                    break;
                case ApiResult.Exception:
                default:
                    api.Exceptions += (total, sql.SqlCount);
                    break;
            }

            TryUpdateJson();
        }

        /// <summary>
        /// 每分钟尝试保存一次Json，并且更新一次统计数据
        /// </summary>
        public void TryUpdateJson()
        {
            if (Watch.AssertTimeEllapse(TimeSpan.FromMinutes(1)))
            {
                Watch.SetCurrent();
                SaveApis();
            }
        }

        #region 辅助函数
        public void SaveApis()
        {
            // using Stream stream = new FileStream(file, FileMode.Create);
            string text = Descriptors.SearializeJson(true);
            try
            {
                File.WriteAllText(file, text);
            }
            catch
            {
                ;
            }
        }

        public List<ApiDescriptor> LoadApis()
        {
            EnsureDirectoryExists(file);
            List<ApiDescriptor> descriptors = [];
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                Dictionary<string, ApiDescriptor> obj = DesearializeJson<Dictionary<string, ApiDescriptor>>(json) ?? [];
                descriptors = obj.Select(e => e.Value).ToList();

                foreach (var item in descriptors)
                {
                    item.Status = item.Status == ApiStatus.Connecting ? ApiStatus.Disconnecting : item.Status;
                }
            }
            return descriptors;
        }

        public void MergeDescriptor(List<ApiDescriptor> descriptors, List<ApiDescriptor> apis)
        {
            foreach (var item in apis)
            {
                var exists = descriptors.FirstOrDefault(e => e.Name == item.Name);
                if (exists == null)
                {
                    descriptors.Add(item);
                }
                else
                {
                    exists.Merge(item);
                }
            }
        }
        #endregion
    }
}
