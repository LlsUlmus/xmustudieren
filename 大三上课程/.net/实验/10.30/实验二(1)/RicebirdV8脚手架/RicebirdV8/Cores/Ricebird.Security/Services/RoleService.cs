using Ricebird.Framework.DataValidator;
using Ricebird.Framework.Security.Apis;

namespace Ricebird.Security.Services
{
    public class RoleService(IServiceProvider provider) : ISingletonDependency
    {
        private FrozenDictionary<Guid, RoleSchema> _schemas = FrozenDictionary<Guid, RoleSchema>.Empty;
        public FrozenDictionary<Guid, RoleSchema> Schemas
        {
            get
            {
                if (_schemas.Count == 0)
                {
                    LoadSchemas();
                }
                return _schemas;
            }
        }

        private RoleSchema? _annoymous = null;
        public RoleSchema Annoymous
        {
            get
            {
                _annoymous ??= new RoleSchema()
                {
                    ID = Guid.Empty,
                    Name = "匿名用户",
                    CanEdit = false,
                    DisplayAs = "",
                    Permissions = [],
                    Menus = [],
                    For = RuleFor.User,
                    NotSetEquals = AuthorizeResult.Deny,
                    DisplayOrder = 9999999
                };
                return _annoymous;
            }
        }

        internal RoleSchema? _super = null;
        public RoleSchema SuperAdministrator
        {
            get
            {
                if (_super == null)
                {
                    _super = new RoleSchema()
                    {
                        ID = new Guid("4f54bb46-8877-46d9-afc5-7b601107be26"),
                        Name = "超级管理员",
                        CanEdit = false,
                        DisplayAs = "",
                        Permissions = [],
                        Menus = [],
                        For = RuleFor.User,
                        NotSetEquals = AuthorizeResult.Access,
                        DisplayOrder = -9999999,
                    };
                    SetMenus(_super, provider);
                    SetPermissions(_super, provider);
                }
                return _super;
            }
        }

        public RoleSchema DefaultSchema
        {
            get; set;
        } = new RoleSchema()
        {
            ID = Guid.Empty
        };

        #region LoadSchemas
        public void LoadSchemas()
        {
            using IServiceScope scope = provider.CreateScope();
            var repo = scope.Resolve<RoleSchemaRepository>();

            var result = repo.DbSet.ToList();
            foreach (var item in result)
            {
                SetMenus(item, scope.ServiceProvider);
                SetPermissions(item, scope.ServiceProvider);

                if (item.IsDefaultRole)
                {
                    DefaultSchema = item;
                }
            }

            repo.SaveChanges();

            var tmp = result.ToFrozenDictionary(e => e.ID, e => e);
            (_schemas, tmp) = (tmp, _schemas);
            tmp.Values.Clear();
        }

        /// <summary>
        /// 必须先调用先调用此函数，再调用SetPermissions。也必须同时调用！
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="scope"></param>
        internal void SetMenus(RoleSchema schema, IServiceProvider scope)
        {
            // 清理所有不存的菜单
            // 菜单里保存的是 菜单的ID！
            MenuService menuService = scope.Resolve<MenuService>();

            List<MenuItem> result = [];
            List<string> ids = [];
            List<string> menus = [];
            foreach (MenuItem lv1 in menuService.AllMenus.Where(e => e.ParentId == Guid.Empty))
            {
                if (!lv1.Children.Any())
                {
                    ids.Add(lv1.ID.ToString());
                    if (schema.Successed(lv1))
                    {
                        result.Add(lv1);
                        menus.Add(lv1.LinkTo);
                    }
                }
                else
                {
                    MenuItem lv1New = lv1.CopyTo();
                    lv1New.Children = new List<MenuItem>();
                    ids.Add(lv1.ID.ToString());
                    foreach (MenuItem lv2 in lv1.Children)
                    {
                        ids.Add(lv2.ID.ToString());
                        if (schema.Successed(lv2))
                        {
                            lv1New.AddChild(lv2);
                            menus.Add(lv2.LinkTo);
                        }
                    }

                    if (lv1New.Children.Any())
                    {
                        result.Add(lv1New);
                    }
                }
            }

            schema.Menus.RemoveAll(e => !ids.Any(x => e.Name == x));
            schema.FinalMenus = result.Select(e => e.ToJsonObject()).ToList();
            schema.FinalPermissions = menus;
        }

        /// <summary>
        /// 必须先调用先调用SetMenus，再调用此函数。也必须同时调用！
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="scope"></param>
        internal void SetPermissions(RoleSchema schema, IServiceProvider scope)
        {
            // 清理所有不存的权限
            ApiManager apis = scope.Resolve<ApiManager>();

            List<AuthorizeDescriptor> needRemove = [];
            foreach (var item in schema.Permissions)
            {
                // 这里所有权限都不能是未设置的
                if (item.Result == AuthorizeResult.NoSet)
                {
                    needRemove.Add(item);
                    continue;
                }

                // 清理掉所有过期的接口
                if (!apis.Connectings.TryGetValue(item.Name, out _))
                {
                    needRemove.Add(item);
                    continue;
                }
            }

            schema.Permissions.RemoveAll(needRemove.Contains);
            List<string> permissions = [];
            List<string> onlyNeedLogined = apis.Connectings.Values.Where(e => e.AuthorizeLevel <= ApiAuthorizeLevel.Login).Select(e => e.Name).ToList();
            schema.FinalPermissions.AddRange(onlyNeedLogined);
            foreach (var item in apis.Connectings)
            {
                // 过一遍所有权限，有效的全部加入
                if (schema.Successed(item.Value))
                {
                    permissions.Add(item.Value.Name);
                }

                if (item.Value.FinalLinkTo.HasValue() && schema.Successed(item.Value.FinalLinkTo))
                {
                    permissions.Add(item.Value.Name);
                }

                if (schema.FinalPermissions.Contains(item.Value.LinkTo) || item.Value.FinalLinkTo.HasValue() && schema.FinalPermissions.Contains(item.Value.FinalLinkTo))
                {
                    permissions.Add(item.Value.Name);
                }
            }

            // 将角色名也视为一个权限加入
            if (!permissions.Any(e => e == schema.Name))
            {
                permissions.Add(schema.Name);
            }

            permissions.AddRange(schema.FinalPermissions);

            schema.FinalPermissions = permissions.Distinct().ToList();

            needRemove.Clear();
        }
        #endregion

        public void EnsureRoleSchema(IClient client, RoleSchema schema)
        {
            var repo = client.Resolve<RoleSchemaRepository>();
            RoleSchema? entity = Schemas.FirstOrDefault(e => e.Value.Name == schema.Name).Value;
            if (entity == null)
            {
                entity = schema;
                repo.DbSet.Add(entity);
            }

            entity.Name = schema.Name;
            entity.CanEdit = false;
            entity.DisplayAs = schema.DisplayAs;
            entity.NotSetEquals = schema.NotSetEquals;
            entity.DisplayOrder = schema.DisplayOrder;
            entity.UseAsPrincipal = schema.UseAsPrincipal;

            repo.SaveChanges();
        }

        #region 角色结构管理
        public RoleSchema? GetRoleSchemaById(Guid id)
        {
            if (Schemas.TryGetValue(id, out var schema))
            {
                return schema;
            }

            if (id == SuperAdministrator.ID)
            {
                return SuperAdministrator;
            }

            if (id == Annoymous.ID)
            {
                return Annoymous;
            }

            return null;
        }

        public RoleSchema? GetSchema(string name)
        {
            var schema = Schemas.FirstOrDefault(e => e.Value.Name == name).Value;
            return schema.Name == name ? schema : null;
        }

        public (bool success, string msg, object data) GetRoleSchemas(bool force)
        {
            if (force || _schemas.Count == 0)
            {
                LoadSchemas();
            }

            var data = Schemas.Values.Select(e => new
            {
                key = e.ID,
                title = e.Name,
                order = e.DisplayOrder,
                canEdit = e.CanEdit,
                isDefault = e.IsDefaultRole,
                asPrincipal = e.UseAsPrincipal,
                displayAs = e.DisplayAs,
                setFor = e.For
            }).OrderBy(e => e.order).ToList();

            return (true, "", data);
        }

        public (bool success, string msg, ValidateResult result, RoleSchema? data) SaveRoleSchema(IClient client, bool force = false)
        {
            RoleSchemaRepository repo = client.Resolve<RoleSchemaRepository>();
            var (_, entity) = repo.FillDeserializeEntity(client);

            var result = entity.Validate(client);
            if (!force && !result)
            {
                return (false, "", result, null);
            }

            repo.SaveChanges();
            LoadSchemas();

            return (true, "", result, entity);
        }

        public (bool success, string msg) RemoveRoleSchema(Guid id, IClient client)
        {
            RoleSchemaRepository repo = client.Resolve<RoleSchemaRepository>();
            var roleSchema = repo.DbSet.FirstOrDefault(e => e.ID == id);
            if (roleSchema != null && roleSchema.CanEdit)
            {
                repo.DbSet.Remove(roleSchema);
                repo.SaveChanges(true);
                LoadSchemas();
            }

            return (true, "");
        }
        #endregion
    }
}
