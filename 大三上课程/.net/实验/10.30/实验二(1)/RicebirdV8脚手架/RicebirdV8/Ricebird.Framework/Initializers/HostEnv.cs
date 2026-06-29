using Ricebird.Framework.Configurations.OptionReaders;
using Ricebird.Framework.Initializers;
using Ricebird.Framework.Security;

namespace Ricebird.Framework
{
    public class HostEnv
    {
        #region ctor
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal HostEnv()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            Counter = Stopwatch.StartNew();
            FileLogger = new FileLogger("./", Counter);
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public static IServiceProvider ServiceProvider
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            get;
            internal set;
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public static HostEnv Instance
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            get;
            internal set;
        }
        #endregion

        #region 环境变量配置
        /// <summary>
        /// 用以保存应用程序执行所需要的资源，比如配置文件等
        /// <para>
        /// 即是程序中的ContentRootPath
        /// </para>
        /// </summary>
        public string AppRootPath
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 取一个绝对地址，该地址指向AppRootPath中的某个目录，即和DLL文件存在一起
        /// <para>
        /// 例：取 %AppRootPath%/saved/service.json 则写为：HostEnv.GetAppPath("saved", "service.json");
        /// </para>
        /// <para>
        /// 如果目录不存在，系统会自动创建目录
        /// </para>
        /// </summary>
        /// <param name="subPaths">各级目录，使用Path.Combine结合</param>
        /// <returns></returns>
        public string GetAppPath(params string[] subPaths)
        {
            return GetPath([AppRootPath, .. subPaths]);
        }

        /// <summary>
        /// 取AppRootPath中的某个目录下文件的所有信息，即和DLL文件存在一起
        /// <para>
        /// 如果目录不存在，则报出异常
        /// </para>
        /// </summary>
        /// <param name="subPaths">各级目录，使用Path.Combine结合</param>
        /// <returns></returns>
        public byte[] ReadAllBytes(params string[] subPaths)
        {
            string path = GetPath(subPaths);
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// 取AppRootPath中的某个目录下文件的所有信息，即和DLL文件存在一起
        /// <para>
        /// 如果目录不存在，则报出异常
        /// </para>
        /// </summary>
        /// <param name="subPaths">各级目录，使用Path.Combine结合</param>
        /// <returns></returns>
        public MemoryStream ReadStream(params string[] subPaths)
        {
            return new MemoryStream(ReadAllBytes(subPaths));
        }

        /// <summary>
        /// 用以保存HTTP请求所需要的资源，比如图片等
        /// <para>
        /// 保存在wwwroot目录下
        /// </para>
        /// </summary>
        public string WebRootPath
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 取一个绝对地址，该地址指向WebRootPath中的某个目录，即保存在wwwroot目录下
        /// <para>
        /// 例：取 %WebRootPath%/attachments/service.txt 则写为：HostEnv.GetAppPath("attachments", "service.txt");
        /// </para>
        /// <para>
        /// 如果目录不存在，系统会自动创建目录
        /// </para>
        /// </summary>
        /// <param name="subPaths">各级目录，使用Path.Combine结合</param>
        /// <returns></returns>
        public string GetWebPath(params string[] subPaths)
        {
            return GetPath([WebRootPath, .. subPaths]);
        }

        public string Environment => FrameworkOptions.EnvironmentName;

        public bool IsDevelopment()
        {
            return IsEnvironment("Development");
        }

        public bool IsProduction()
        {
            return IsEnvironment("Production");
        }

        public bool IsStaging()
        {
            return IsEnvironment("Staging");
        }

        public bool IsEnvironment(string env)
        {
            return Environment == env;
        }
        #endregion

        #region 模块初始化
        internal ModuleInitializer ModuleInitializer { get; set; }

        public List<Type> AllEntities => ModuleInitializer.AllEntities;

        public List<WebModule> AllWebModules => ModuleInitializer.WebModules;

        public List<Type> DataDictionaries => ModuleInitializer.DataDictionaries;

        public void AddAllModules(IServiceCollection services)
        {
            WriteLog("初始化器", "开始扫描被引用类库的IoC关系");
            ModuleInitializer = new ModuleInitializer(FrameworkOptions.Modules, services, this);
            ModuleInitializer.InitializeAssembly();
            RepositoryInitializer repositoryInitializer = new RepositoryInitializer(AllEntities);
            repositoryInitializer.Initialize(services);
            foreach (var module in AllWebModules)
            {
                module.HostEnv = this;
                module.Register(services);
                WriteLog($"{module.Name}", $"{module.DisplayName}注册完成");
            }
        }

        /// <summary>
        /// 初始化所有模块，初始化时间点在路由创建之前，MVC初始化之后
        /// </summary>
        /// <param name="app"></param>
        public void UseRicebirdModules(WebApplication app)
        {
            foreach (var m in AllWebModules)
            {
                m.Use(app);
                WriteLog($"{m.Name}", $"{m.DisplayName}初始化完成");
            }
        }
        #endregion

        #region 网站基本配置
        public RicebirdFrameworkOptions FrameworkOptions
        {
            get; set;
        } = new RicebirdFrameworkOptions();

        public RicebirdFrameworkOptions AddOptions()
        {
            LocalOptionProvider reader = new LocalOptionProvider();
            FrameworkOptions = reader.LoadOptions(FrameworkOptions);
            string dev = Environment switch
            {
                "Development" => "测试",
                "Production" => "生产",
                "Staging" => "演示",
                _ => "异常",
            };
            WriteLog("初始化器", $"当前运行环境为：{dev}（{Environment}）");
            reader.SaveOptions(FrameworkOptions);

            return FrameworkOptions;
        }
        #endregion

        #region 文件日志记录
        public Stopwatch Counter
        {
            get; init;
        }

        private FileLogger FileLogger
        {
            get; init;
        }

        public void WriteLog(string module, string log)
        {
            FileLogger?.WriteLog(module, log);
        }

        public void InitialEnd()
        {
            FileLogger?.InitialEnd();
        }
        #endregion

        public void SetServiceProvider(IServiceProvider services) => ServiceProvider = services;

        public ICredentialService CredentialService
        {
            get;
            set;
        }

    }
}
