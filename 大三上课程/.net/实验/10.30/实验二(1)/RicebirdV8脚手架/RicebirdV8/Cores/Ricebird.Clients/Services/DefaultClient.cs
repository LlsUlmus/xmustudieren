using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ricebird.Clients.Models;
using Ricebird.Framework.Configurations;
using Ricebird.Framework.Diagnostics;
using Ricebird.Framework.Diagnostics.Features;
using Ricebird.Framework.Security;
using Ricebird.Framework.SystemExtensions;
using Ricebird.Framework.Tools.JsonConverters;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ricebird.Clients.Services
{
    internal class DefaultClient : IClient
    {
        private readonly Guid _clientId = Guid.NewGuid();
        public Guid Id => _clientId;

        public ClientType Type { get; set; }
        public IDbLogger Logger { get; set; }

        public RicebirdFrameworkOptions Options { get; set; }

        #region ctor
        public DefaultClient(IServiceProvider provider, Framework.Diagnostics.IDbLogger logger)
        {
            Type = ClientType.Anonymous;
            Services = provider;
            _serviceKey = Guid.Empty;
            _serviceName = string.Empty;
            Features = new ClientFeatureCollection(this);
            Logger = logger;

            var hostEnv = Services.Resolve<HostEnv>();
            Options = hostEnv.FrameworkOptions;
        }

        public void BuildClient(HttpContext ctx)
        {
            HttpContext = ctx;
            Type = ClientType.Anonymous;
            Services = ctx.RequestServices;
            _serviceKey = ServiceKeys.ClientSericeKey;
            _serviceName = "客户端服务";

            var hostEnv = Services.Resolve<HostEnv>();
            Options = hostEnv.FrameworkOptions;
            LoadWorkContext();
        }

        public void BuildClient(IServiceScope scope, Guid sKey, string sName)
        {
            Type = ClientType.Module;
            Services = scope.ServiceProvider;
            _serviceKey = sKey;
            _serviceName = sName;
            Logger = Resolve<IDbLogger>();
        }
        #endregion

        #region 构造相关
        public bool Equals(IClient? other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return other.Id == Id;
        }
        #endregion

        #region 读取选项
        public T LoadOptions<T>() where T : IOption, new() => Services.LoadOptions<T>();
        #endregion

        #region 当前用户
        public IUserPrincipal CurrentUser
        {
            get
            {
                switch (Type)
                {
                    case ClientType.Module:
                        return Features.Get<IUserPrincipal>() ?? new SystemUserEntity(_serviceKey, _serviceName);
                    case ClientType.SignIn:
                    case ClientType.Anonymous:
                    default:
                        var user = Features.Get<IUserPrincipal>() ?? throw new NotSupportedException($"必须先加载Ricebird.Authentication模块，才可以使用本功能");
                        return user;
                }
            }
        }

        public bool Successed(string permission)
        {
            return Type switch
            {
                ClientType.Module => true,
                _ => CurrentUser.Succeed(permission),
            };
        }

        public bool Successed(IEnumerable<string> permissions)
        {
            return Type switch
            {
                ClientType.Module => true,
                _ => CurrentUser.Succeed(permissions),
            };
        }
        #endregion

        #region 请求中载入的数据
        public void LoadWorkContext()
        {
            if (Request == null)
            {
                return;
            }

            Request.EnableBuffering();

            #region 读取请求流
            int len = (int)(Request.ContentLength ?? 0);
            if (len > 0 && !(Request.ContentType?.Contains("multipart/form-data") ?? false))
            {
                BodyStream = new MemoryStream();
                Request.Body.CopyToAsync(BodyStream).GetAwaiter().GetResult();
                Request.Body = BodyStream;
                BodyStream.Seek(0, SeekOrigin.Begin);
                try
                {
                    int length = (int)Request.Body.Length;
                    byte[] bytes = new byte[length];
                    Request.Body.ReadExactly(bytes, 0, length);
                    PostStream = Encoding.UTF8.GetString(bytes);

                    //在转换为PostStream后，测试能否转为JSON
                    _simpleJsonData = Utils.ReadJsonLv1(bytes).ToFrozenDictionary();
                }
                catch
                {
                    // 有问题就不管了，因为有可能输入的字符串而不是JSON
                }
                finally
                {
                    Request.Body.Seek(0, SeekOrigin.Begin);
                }
            }
            #endregion
        }
        public MemoryStream BodyStream
        {
            get; set;
        } = new MemoryStream();

        public byte[] PayloadBytes
        {
            get; set;
        } = [];

        public string PostStream
        {
            get; set;
        } = string.Empty;

        private FrozenDictionary<string, object> _simpleJsonData = (new Dictionary<string, object>()).ToFrozenDictionary();

        public Dictionary<string, object> ClientData
        {
            get; set;
        } = [];

        public void MergeData(string key, object data)
        {
            ClientData.MergeKey(key, data);
        }

        #region 参数项统计
        private FrozenDictionary<string, object>? _paramFinder = null;
        private readonly List<string> contentTypeAllowForm = ["application/x-www-form-urlencoded", "multipart/form-data"];
        private FrozenDictionary<string, object> ParamFinder
        {
            get
            {
                if (_paramFinder == null)
                {
                    var dict = new Dictionary<string, object>();

                    dict.MergeDictionary(_simpleJsonData);

                    // 将参数项合并
                    if (Request != null)
                    {
                        if (Request.RouteValues != null && Request.RouteValues.Count > 0)
                        {
                            foreach (var routeItem in Request.RouteValues)
                            {
                                dict.MergeKey(routeItem.Key, routeItem.Value);
                            }
                        }

                        if (contentTypeAllowForm.Any(e => (Request.ContentType ?? "").Contains(e)) && Request.Form != null && Request.Form.Count > 0)
                        {
                            foreach (var formItem in Request.Form)
                            {
                                dict.MergeKey(formItem.Key, formItem.Value);
                            }
                        }

                        foreach (var headerItem in Request.Headers)
                        {
                            dict.MergeKey(headerItem.Key, headerItem.Value);
                        }

                        foreach (var queryItem in Request.Query)
                        {
                            dict.MergeKey(queryItem.Key, queryItem.Value);
                        }
                    }

                    _paramFinder = dict.ToFrozenDictionary();
                }
                return _paramFinder;
            }
        }
        #endregion

        private bool PrivateGet<T>(string paramName, T defaultValue, [NotNullWhen(true)] out T? value)
        {
            if (ClientData.TryGetValue(paramName, out object? v))
            {
                value = ValueUtils.ChangeToType(v, defaultValue);
                return value != null;
            }

            if (ParamFinder.TryGetValue(paramName, out v))
            {
                value = ValueUtils.ChangeToType(v, defaultValue);
                return value != null;
            }

            value = default;
            return false;
        }

        private bool PrivateGet<T>(string paramName, Type valueType, [NotNullWhen(true)] out object? value)
        {
            if (ClientData.TryGetValue(paramName, out object? v))
            {
                value = ValueUtils.ChangeToType(v, valueType.GetDefaultValue());
                return value != null;
            }

            if (ParamFinder.TryGetValue(paramName, out v))
            {
                value = ValueUtils.ChangeToType(v, valueType.GetDefaultValue());
                return value != null;
            }

            value = default;
            return false;
        }

        private string ParameterNameChange(string paramName)
        {
            if (paramName.Length > 1 && paramName[0] is >= 'A' and <= 'Z')
            {
                char first = (char)(paramName[0] + 32); // 字符转小写
                string camelCase = new string([first, .. paramName[1..]]);
                camelCase = paramName.ToUpper() == "ID" ? "id" : camelCase;
                return camelCase;
            }

            if (paramName.Length > 1 && paramName[0] is >= 'a' and <= 'z')
            {
                char first = (char)(paramName[0] - 32); // 字符转大写
                string pascalCase = new string([first, .. paramName[1..]]);
                pascalCase = paramName.ToLower() == "id" ? "ID" : pascalCase;
                return pascalCase;
            }

            return string.Empty;
        }

        public bool TryGet<T>(string paramName, T defaultValue, [NotNullWhen(true)] out T? value)
        {
            if (PrivateGet(paramName, defaultValue, out value))
            {
                return true;
            }

            string changeTo = ParameterNameChange(paramName);
            if (changeTo != string.Empty)
            {
                return PrivateGet(changeTo, defaultValue, out value);
            }

            value = default;
            return false;
        }

        public bool TryGet(string paramName, Type valueType, [NotNullWhen(true)] out object? value)
        {
            if (PrivateGet(paramName, valueType, out value))
            {
                return true;
            }

            string changeTo = ParameterNameChange(paramName);
            if (changeTo != string.Empty)
            {
                return PrivateGet(changeTo, valueType, out value);
            }

            value = default;
            return false;
        }

        public T GetInRequest<T>(string paramName, T defaultValue)
        {
            if (TryGet(paramName, defaultValue, out T? value))
            {
                return value;
            }

            return defaultValue;
        }

        public T Get<T>(string paramName, T defaultValue)
        {
            if (TryGet(paramName, defaultValue!, out T? value))
            {
                return value;
            }

            if (Request != null && Request.Cookies.TryGetValue(paramName, out string? str))
            {
                T cookieValue = ValueUtils.ChangeToType<T>(str, defaultValue)!;
                return cookieValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 取得拥有mutiple属性的下拉框的属性值，并且将其全部转换为指定类型
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public List<T> GetList<T>(string paramName, string seperator)
        {
            try
            {
                string txt = Get(paramName, "");

                List<T> result = txt.Split(seperator, StringSplitOptions.RemoveEmptyEntries).Select(e => ValueUtils.ChangeToType<T>(e)!).ToList();

                return result;
            }
            catch
            {
                return [];
            }

        }

        public void FillObject<T>(T obj)
        {
            FillObject(obj, []);
        }

        public ModelBindingResult BindEntity(Type entityType)
        {
            var obj = Services.GetRequiredService(entityType);

            foreach (PropertyInfo prop in entityType.GetProperties())
            {
                if (!prop.CanWrite)
                {
                    continue;
                }

                if (typeof(IServiceProvider).IsAssignableFrom(prop.PropertyType))
                {
                    continue;
                }

                object? defaultValue = prop.GetValue(obj) ?? prop.PropertyType.GetDefaultValue();
                if (!TryGet(prop.Name, defaultValue, out object? value))
                {
                    continue;
                }

                if (value is JsonElement je)
                {
                    switch (je.ValueKind)
                    {
                        case JsonValueKind.String:
                        case JsonValueKind.Number:
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            value = je.ToString();
                            break;
                        case JsonValueKind.Object:
                        case JsonValueKind.Array:
                        case JsonValueKind.Undefined:
                        case JsonValueKind.Null:
                        default:
                            continue;
                    }
                }

                object v = ValueUtils.ChangeToType(value, prop.PropertyType, new object());
                prop.SetValue(obj, v);
            }

            if (obj is EntityBase entity)
            {
                entity.Client = this;
                entity.BindClientData(this);
            }

            return ModelBindingResult.Success(obj);
        }


        public void FillObject<T>(T obj, params string[] ignoreProperties)
        {
            T? res = default;
            try
            {
                res = Deserialize<T>();
                if (res == null)
                {
                    throw new InvalidCastException("无法将JSON转换为此类型的内容");
                }

                var props = typeof(T).GetProperties();
                foreach (var item in ClientData)
                {
                    PropertyInfo? prop = props.FirstOrDefault(e => e.Name == item.Key);
                    if (prop == null) continue;

                    if (!prop.CanWrite || !prop.CanRead)
                    {
                        continue;
                    }

                    if (typeof(IServiceProvider).IsAssignableFrom(prop.PropertyType))
                    {
                        continue;
                    }

                    if (ignoreProperties.Contains(prop.Name))
                    {
                        continue;
                    }

                    object v = ValueUtils.ChangeToType(item.Value, prop.PropertyType, new object());
                    prop.SetValue(obj, v);
                }
            }
            catch
            {
                ;
            }

            if (res == null)
            {
                var props = typeof(T).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (!prop.CanWrite || !prop.CanRead)
                    {
                        continue;
                    }

                    if (typeof(IServiceProvider).IsAssignableFrom(prop.PropertyType))
                    {
                        continue;
                    }

                    if (ignoreProperties.Contains(prop.Name))
                    {
                        continue;
                    }

                    object? defaultValue = prop.GetValue(obj) ?? prop.PropertyType.GetDefaultValue();
                    if (!TryGet(prop.Name, defaultValue, out object? value))
                    {
                        continue;
                    }

                    object v = ValueUtils.ChangeToType(value, prop.PropertyType, new object());
                    prop.SetValue(obj, v);
                }
            }
            else
            {
                obj!.CopyPropertiesFrom(res);
            }

            if (obj is EntityBase entity)
            {
                entity.Client = this;
                entity.BindClientData(this);
            }
        }

        public TEntity FillResolveObject<TEntity>()
            where TEntity : class
        {
            TEntity obj = Resolve<TEntity>();
            FillObject(obj);
            return obj;
        }

        public TEntity FillResolveObject<TEntity>(params string[] ignoreProperties)
            where TEntity : class
        {
            TEntity obj = Resolve<TEntity>();
            FillObject(obj, ignoreProperties);
            return obj;
        }

        public TEntity? Deserialize<TEntity>()
        {
            TEntity? entity = JsonSerializer.Deserialize<TEntity>(PostStream, ClientJsonOption);
            if (entity is EntityBase eb)
            {
                eb.Client = this;
            }
            return entity;
        }

        private static JsonSerializerOptions ClientJsonOption
        {
            get
            {
                JsonSerializerOptions _default = new JsonSerializerOptions(RicebirdSerializerOption.Default);
                //_default.AddConverter<Int32Converter>();
                //_default.AddConverter<Int64Converter>();
                _default.Converters.Add(new RicebirdDateTimeConverter("yyyy-M-d"));
                _default.PropertyNameCaseInsensitive = true;
                return _default;
            }
        }

        public bool IsWeixinClientRequest
        {
            get
            {
                return !string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("MicroMessenger");
            }
        }
        #endregion

        #region 客户端相关的信息
        private string _realIp = string.Empty;
        public string RealIp
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_realIp))
                {
                    string forwardIp = Get("X-Forwarded-For", "");
                    if (forwardIp.HasValue())
                    {
                        _realIp = forwardIp;
                        return _realIp;
                    }

                    string remoteIp = Request?.HttpContext.Connection.RemoteIpAddress?.ToString() ?? forwardIp;
                    string ip = Get("X-Real-IP", remoteIp);
                    _realIp = ip;
                }

                return _realIp;
            }
        }

        private string _serviceName;

        private string _userAgent = string.Empty;
        public string UserAgent
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_userAgent))
                {
                    _userAgent = (Request?.Headers.UserAgent.ToString()) ?? ($"RicebirdFramework/8.0 {_serviceName}");
                }
                return _userAgent;
            }
        }

        private string _apiPath = string.Empty;
        public string ApiPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_apiPath))
                {
                    _apiPath = Request?.Path ?? _serviceName;
                }
                return _apiPath;
            }
        }

        private string _method = string.Empty;
        public string Method
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_method))
                {
                    _method = Request?.Method ?? "InSys";
                }
                return _method;
            }
        }

        private Guid _serviceKey;
        private string _displayUrl = string.Empty;
        public string DisplayUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_displayUrl))
                {
                    _displayUrl = Request?.GetDisplayUrl() ?? _serviceKey.ToString();
                }
                return _displayUrl;
            }
        }

        private string _hostWithScheme = string.Empty;
        public string HostWithScheme
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_hostWithScheme))
                {
                    _hostWithScheme = $"{Request?.Scheme}://{Request?.Host.Value}" ?? $"fsp://{_serviceKey}";
                }
                return _hostWithScheme;
            }
        }

        private List<string>? _ipRegions = null;
        public List<string> IpRegions
        {
            get
            {
                if (_ipRegions == null)
                {
                    if (!IPAddress.TryParse(RealIp, out IPAddress? ip))
                    {
                        return [];
                    }

                    _ipRegions = [];
                    IOptionService oService = Resolve<IOptionService>();
                    var opt = oService.LoadOptions<IpRegionOption>();
                    foreach (var region in opt.Regions)
                    {
                        if (region.Contains(ip))
                        {
                            _ipRegions.Add(region.Name);
                        }
                    }
                }

                return _ipRegions;
            }
        }

        public string Params => ParamFinder.Select(e => $"{e.Key}={e.Value}").JoinAsString('&');
        #endregion

        #region Cookie相关函数
        /// <summary>
        /// 设置本地cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>  
        /// <param name="minutes">过期时长，单位：分钟</param>      
        [Obsolete("已过期，请使用localStorage保存数据")]
        public void SetCookies(string key, string value, int minutes = 30)
        {
            if (HttpContext == null)
            {
                return;
            }

            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(minutes)
            });
        }
        /// <summary>
        /// 删除指定的cookie
        /// </summary>
        /// <param name="key">键</param>
        [Obsolete("已过期，请使用localStorage保存数据")]
        public void DeleteCookies(string key)
        {
            if (HttpContext == null)
            {
                return;
            }

            HttpContext.Response.Cookies.Delete(key);
        }

        /// <summary>
        /// 获取cookies
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        [Obsolete("已过期，请使用localStorage保存数据")]
        public string GetCookies(string key)
        {
            if (HttpContext == null)
            {
                return "";
            }

            HttpContext.Request.Cookies.TryGetValue(key, out string? value);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
        }
        #endregion

        #region 日志接口
        public void LogException(Exception ex, string module, string method)
        {
            Logger.LogException(this, ex, module, method);
        }

        public void Log(string module, string method, string relateId, string desc)
        {
            Logger.Log(this, module, method, relateId, desc);
        }
        #endregion

        #region 服务相关内容
        public HttpContext? HttpContext
        {
            get;
            internal set;
        }

        public HttpRequest? Request => HttpContext?.Request;

        public IServiceProvider Services
        {
            get;
            internal set;
        }

        public IFeatureCollection Features
        {
            get; init;
        }

        public Browser Browser
        {
            get
            {
                Browser? browser = Features.Get<Browser>();
                browser ??= new Browser("RicebirdFramework");
                return browser;
            }
        }

        public T Resolve<T>()
            where T : class
        {
            T? f = Features.Get<T>();
            if (f != null)
            {
                return f;
            }

            T obj = Services.Resolve<T>();
            if (obj is EntityBase eb)
            {
                eb.Client = this;
            }

            return obj;
        }

        public T Resolve<T>(string productName)
            where T : class
        {
            T obj = Services.Resolve<T>(productName);
            if (obj is EntityBase eb)
            {
                eb.Client = this;
            }
            return obj;
        }
        #endregion

        #region 复制一个新的Client
        public IClient Clone(IServiceScope scope, Guid sKey, string sName)
        {
            DefaultClient client = (scope.Resolve<IClient>() as DefaultClient)!;
            client.BuildClient(scope, sKey, sName);

            // 用户数据复制
            var currentUser = CurrentUser;
            client.Features.Set<IUserPrincipal>(currentUser);

            // 请示数据复制
            client.PayloadBytes = PayloadBytes;
            client.BodyStream = new MemoryStream(PayloadBytes);
            client._simpleJsonData = _simpleJsonData;
            client.ClientData = ClientData;
            client._paramFinder = _paramFinder;
            client._realIp = RealIp;
            client._userAgent = UserAgent;
            client._apiPath = ApiPath;
            client._method = Method;
            client._displayUrl = DisplayUrl;
            client._hostWithScheme = HostWithScheme;
            client.Features.Set(Browser);

            client.Features.Set<IClient>(this);
            return client;
        }
        #endregion

        public void Dispose()
        {
            (Features as ClientFeatureCollection)?.Dispose();
            PayloadBytes = [];
            BodyStream.Dispose();
            _simpleJsonData = (new Dictionary<string, object>()).ToFrozenDictionary();
            ClientData.Clear();
            _paramFinder = null;
            _realIp = string.Empty;
            _userAgent = string.Empty;
            _apiPath = string.Empty;
            _method = string.Empty;
            _displayUrl = string.Empty;
            _hostWithScheme = string.Empty;
        }
    }
}
