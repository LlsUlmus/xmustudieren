
namespace Ricebird.Framework.Security.Apis
{
    public class ApiDescriptor : IComparable<ApiDescriptor>
    {
        #region ctor
        public ApiDescriptor() { }
        public ApiDescriptor(WebModule module)
        {
            Name = module.DisplayName;
            ApiType = ApiType.Module;
            Status = ApiStatus.Connecting;
            DisplayOrder = module.Priority;
            Module = module.DisplayName;
            ApiGroup = "";
            AuthorizeLevel = ApiAuthorizeLevel.None;
        }

        public ApiDescriptor(WebModule module, string permission)
        {
            Name = permission;
            ApiType = ApiType.Permission;
            Status = ApiStatus.Connecting;
            DisplayOrder = module.Priority;
            Module = module.DisplayName;
            ApiGroup = "";
            AuthorizeLevel = ApiAuthorizeLevel.Authorize;
        }

        public ApiDescriptor(WebModule module, ApiGroupAttribute group, Api api, string controllerName, string actionName)
        {
            Name = api.Name;
            ApiType = ApiType.WebApi;
            Status = ApiStatus.Connecting;
            DisplayOrder = module.Priority;
            Module = module.DisplayName;
            ApiGroup = group.Name;
            AuthorizeLevel = api.AuthorizeLevel;
            Controller = controllerName;
            Action = actionName;
            if (api.Name != api.Permission)
            {
                LinkTo = api.Permission;
            }
        }
        #endregion

        #region 字段
        /// <summary>
        /// Api名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 权限连接至
        /// </summary>
        public string LinkTo { get; set; } = string.Empty;
        /// <summary>
        /// 链接权限的最终指向
        /// </summary>
        [JsonIgnore]
        public string FinalLinkTo { get; set; } = string.Empty;
        /// <summary>
        /// Api类型
        /// </summary>
        public ApiType ApiType { get; set; } = ApiType.Module;
        /// <summary>
        /// 当前API的状态
        /// </summary>
        public ApiStatus Status { get; set; } = ApiStatus.Disconnecting;
        /// <summary>
        /// 升序排序号
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        /// <summary>
        /// 网络接口所在模块名
        /// </summary>
        public string Module { get; set; } = string.Empty;
        /// <summary>
        /// Api所在组
        /// </summary>
        public string ApiGroup { get; set; } = string.Empty;
        /// <summary>
        /// 接口的访问级别
        /// </summary>
        public ApiAuthorizeLevel AuthorizeLevel { get; set; } = ApiAuthorizeLevel.None;
        /// <summary>
        /// 网络接口对应的控制器名称
        /// </summary>
        [JsonIgnore]
        public string Controller { get; set; } = string.Empty;
        /// <summary>
        /// 网络接口所在Action名称
        /// </summary>
        [JsonIgnore]
        public string Action { get; set; } = string.Empty;
        /// <summary>
        /// 网络接口访问成功时计数
        /// </summary>
        public ApiCounter Success { get; set; } = new();
        /// <summary>
        /// 网络接口访问失败时计数
        /// </summary>
        public ApiCounter Failure { get; set; } = new();
        /// <summary>
        /// 网络接口访问异常时计数
        /// </summary>
        public ApiCounter Exceptions { get; set; } = new();
        #endregion

        public ApiDescriptor Merge(ApiDescriptor other)
        {
            if (Name != other.Name || Module != other.Module /*|| ApiGroup != other.ApiGroup*/)
            {
                throw new InvalidDataException("不允许出现两个同名的功能");
            }

            (ApiType, Status, DisplayOrder, AuthorizeLevel, Controller, Action, LinkTo, ApiGroup)
                = (other.ApiType, other.Status, other.DisplayOrder, other.AuthorizeLevel, other.Controller, other.Action, other.LinkTo, other.ApiGroup);

            Success += other.Success;
            Failure += other.Failure;
            Exceptions += other.Exceptions;

            return this;
        }

        public int CompareTo(ApiDescriptor? other)
        {
            if (other == null) return -1;

            // 返回 -1 排序是 [this, other]
            // 返回 0 说明是相同
            // 返回 1 排序是 [other, this]
            return DisplayOrder > other.DisplayOrder ? 1 : -1;
        }

        public override string ToString() => ApiType switch
        {
            ApiType.Module => $"模块：{Name}",
            ApiType.WebApi => $"接口：{Module} > {Name}",
            ApiType.Permission => $"权限：{Module} > {Name}",
            _ => $"未知：{Name}",
        };
    }

    public class ApiCounter
    {
        public long Count { get; set; }
        public long TotalEllapsed { get; set; }
        public long UseSqlCount { get; set; }
        public long SqlUse { get; set; }

        public ApiCounter() : this((0, 0, 0, 0)) { }
        public ApiCounter((int count, int total, int useSqlCount, int sqlUse) counter) => (Count, TotalEllapsed, UseSqlCount, SqlUse) = counter;

        public static implicit operator ApiCounter((int count, int total, int useSqlCount, int sql) counter) => new ApiCounter(counter);

        public static ApiCounter operator +(ApiCounter left, ApiCounter? other)
        {
            ApiCounter right = other ?? (0, 0, 0, 0);
            left.Count += right.Count;
            left.TotalEllapsed += right.TotalEllapsed;
            left.UseSqlCount += right.UseSqlCount;
            left.SqlUse += right.SqlUse;
            return left;
        }

        public static ApiCounter operator +(ApiCounter left, (int total, int sql) right)
        {
            left.Count++;
            left.TotalEllapsed += right.total;
            if (right.sql > 0)
            {
                left.UseSqlCount++;
                left.SqlUse += right.sql;
            }
            return left;
        }
    }
}
