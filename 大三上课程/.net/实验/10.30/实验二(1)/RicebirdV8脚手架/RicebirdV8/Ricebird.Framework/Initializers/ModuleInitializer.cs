using Microsoft.AspNetCore.Mvc;
using Ricebird.Framework.Database;
using Ricebird.Framework.Security;

namespace Ricebird.Framework.Initializers
{
    public class ModuleInitializer
    {
        public IEnumerable<string> Modules
        {
            get; set;
        } = [];

        public List<Type> AllEntities
        {
            get; set;
        } = [];

        public List<Type> DataDictionaries
        {
            get; set;
        } = [];

        public List<WebModule> WebModules
        {
            get; set;
        }

        internal List<Type> ApiControllers
        {
            get; set;
        } = [];

        internal List<Type> Permissions
        {
            get; set;
        } = [];

        private IServiceCollection Services
        {
            get; set;
        }

        private HostEnv HostEnv
        {
            get;
            init;
        }

        internal ModuleInitializer(IEnumerable<string> modules, IServiceCollection services, HostEnv hostEnv)
        {
            Modules = modules;
            Services = services;
            HostEnv = hostEnv;
            WebModules = [];
        }

        private void WriteLog(string log)
        {
            HostEnv.WriteLog("ModuleInitializer", log);
        }

        /// <summary>
        /// 初始化程序集
        /// </summary>
        internal List<Assembly> InitializeAssembly()
        {
            WriteLog("正在载入程序集");

            List<Assembly> assemblies = [];
            foreach (string item in Modules)
            {
                try
                {
                    var assemblyName = new AssemblyName(item);
                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                    WriteLog($"成功加载程序集{item}");
                }
                catch (Exception ex)
                {
                    WriteLog($"加载程序集{item}时发生异常，{ex.Message}\r\n{ex.StackTrace}");
                }
            }

            List<Type> registerTypes = [];
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                registerTypes.AddRange(assembly.DefinedTypes.Select(e => e.AsType()));
            }

            // 开始初始化程序集
            List<Type> singleton = [];
            List<Type> transient = [];
            List<Type> scoped = [];
            // 扫描一次所有程序集，直接分类
            int i = 0;
            foreach (var type in registerTypes)
            {
                string name = type.Name;
                if (typeof(IDependency).IsAssignableFrom(type))
                {
                    if (typeof(EntityBase).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && type.GetCustomAttribute<DontAutoRegistion>() == null)
                    {
                        AllEntities.Add(type);
                    }

                    transient.Add(type);
                }
                else if (typeof(ISingletonDependency).IsAssignableFrom(type))
                {
                    singleton.Add(type);
                }
                else if (typeof(IScopedDependency).IsAssignableFrom(type))
                {
                    scoped.Add(type);
                }
                else if (typeof(WebModule).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                {
                    if (Activator.CreateInstance(type) is not WebModule module)
                    {
                        throw new InvalidOperationException($"无法实例化{type.Name}");
                    }
                    module.Assembly = type.Assembly;
                    WebModules.Add(module);
                }
                else if (type.IsDefined(typeof(DataDictionaryAttribute), false))
                {
                    DataDictionaries.Add(type);
                }
                else if (type.IsDefined(typeof(ApiGroupAttribute), false) && typeof(Controller).IsAssignableFrom(type))
                {
                    // 有ApiGroup的组才处理
                    ApiControllers.Add(type);
                }
                else if (type.Name == "Permissions")
                {
                    Permissions.Add(type);
                }

                i++;
            }
            WriteLog($"已经搜索程序集中包含的{i}个类，共有程序模块{WebModules.Count}个，单例{singleton.Count}个，域内单例{scoped.Count}个，临时类型{transient.Count}个，实体{AllEntities.Count}个和{DataDictionaries.Count}个内置字典。");

            // 程序集扫描完毕后，开始注册
            RegisterType(transient, Services.AddTransient);
            RegisterType(scoped, Services.AddScoped);
            RegisterType(singleton, Services.AddSingleton);
            WriteLog($"程序集自动注册完毕");
            WebModules = [.. WebModules.OrderBy(e => e.Priority)];
            GenerateApiDescriptor();
            GeneratePermissions();
            return assemblies;
        }

        internal static void RegisterType(List<Type> types, Func<Type, Type, IServiceCollection> registerAction)
        {
            foreach (var t in types)
            {
                if (t.IsClass && !t.IsAbstract && !t.IsGenericType)
                {
                    if (t.GetCustomAttribute<DontAutoRegistion>() != null)
                    {
                        continue;
                    }

                    registerAction(t, t);
                    // 类型必须是可以实例化的，则开始将它注册到它的所有母类上
                    for (Type? baseType = t.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                    {
                        if (baseType == null)
                        {
                            break;
                        }

                        registerAction(baseType, t);
                    }

                    var interfaces = t.GetInterfaces();
                    foreach (var inter in interfaces)
                    {
                        registerAction(inter, t);
                    }
                }
            }
        }

        internal void GenerateApiDescriptor()
        {
            foreach (var item in ApiControllers)
            {
                WebModule? module = WebModules.FirstOrDefault(e => e.Assembly == item.Assembly);
                if (module == null) continue;

                if (!module.Apis.Any(e => e.ApiType == ApiType.Module))
                {
                    module.Apis.Add(new Security.Apis.ApiDescriptor(module));
                }

                ApiGroupAttribute apiGroup = item.GetCustomAttribute<ApiGroupAttribute>()!;
                foreach (var action in item.GetMethods())
                {
                    Api? apiAttr = action.GetCustomAttribute<Api>();
                    if (apiAttr == null) continue;

                    module.Apis.Add(new Security.Apis.ApiDescriptor(module, apiGroup, apiAttr, item.Name, action.Name));
                }
            }
        }

        internal void GeneratePermissions()
        {
            foreach (var item in Permissions)
            {
                WebModule? module = WebModules.FirstOrDefault(e => e.Assembly == item.Assembly);
                if (module == null) continue;

                foreach (var permission in item.GetFields())
                {
                    if (permission.IsLiteral && permission.FieldType == typeof(string) && permission.IsPublic)
                    {
                        // 常数的string就是目标
                        module.Apis.Add(new Security.Apis.ApiDescriptor(module, permission.GetValue(null)?.ToString() ?? ""));
                    }
                }
            }
        } // function
    }
}
