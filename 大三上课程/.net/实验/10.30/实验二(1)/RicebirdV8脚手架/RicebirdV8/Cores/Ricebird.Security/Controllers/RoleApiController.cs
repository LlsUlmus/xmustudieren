namespace Ricebird.Security.Controllers
{
    [Route("~/api/roles/[action]"), ApiGroup("角色管理")]
    public class RoleApiController(RoleService roleService) : ApiController
    {
        #region 角色管理
        [ApiLinkTo("获取角色列表", "/manage/security/roles")]
        public ActionResult GetRoleSchema()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            RoleSchema data = roleService.GetRoleSchemaById(id) ?? new RoleSchema()
            {
                ID = id,
            };

            var validator = data.BuildValidator();

            return Ok(new
            {
                success = true,
                msg = "",
                data,
                rules = validator.ToJsonObject()
            });
        }

        [ApiShouldLogin("获取角色")]
        public ActionResult GetRoleSchemas()
        {
            var (success, msg, data) = roleService.GetRoleSchemas(false);

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiShouldAuthorize("保存角色")]
        public ActionResult SaveRoleSchema()
        {
            var (success, msg, result, data) = roleService.SaveRoleSchema(Client, false);

            if (!result)
            {
                return ValidateError(result);
            }

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiShouldAuthorize("保存角色权限")]
        public ActionResult SavePermissions()
        {
            Submit? value = Client.Deserialize<Submit>();
            if (value == null)
            {
                return Fail("无法解析输入的内容");
            }

            var role = roleService.GetRoleSchemaById(value.id);
            if (role == null)
            {
                return Fail("找不到ID对应的角色，或者输入的字符串解析错误");
            }

            role.Menus = value.menus;
            roleService.SetMenus(role, Client.Services);
            role.Permissions = value.permits;
            roleService.SetPermissions(role, Client.Services);

            RoleSchemaRepository repo = Resolve<RoleSchemaRepository>();
            var r = repo.DbSet.FirstOrDefault(e => e.ID == role.ID);
            if (r != null)
            {
                r.Permissions = role.Permissions;
                r.Menus = role.Menus;
                repo.SaveChanges();
            }

            return Ok(new
            {
                success = true,
                msg = "",
                data = role
            });
        }

        [ApiShouldAuthorize("复制角色")]
        public ActionResult CopyRoleSchema()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            var role = roleService.GetRoleSchemaById(id);
            if (role == null || id == roleService.Annoymous.ID || id == roleService.SuperAdministrator.ID)
            {
                return Fail("找不到ID对应的角色，或者输入的字符串解析错误");
            }

            RoleSchemaRepository repo = Resolve<RoleSchemaRepository>();
            RoleSchema copy = new RoleSchema()
            {
                For = role.For,
                IsDefaultRole = false,
                Menus = role.Menus,
                Name = $"{role.Name}-复制",
                NotSetEquals = role.NotSetEquals,
                Permissions = role.Permissions,
                DisplayAs = role.DisplayAs,
                CanEdit = true,
                DisplayOrder = role.DisplayOrder,
                UseAsPrincipal = role.UseAsPrincipal,
            };
            repo.DbSet.Add(copy);
            repo.SaveChanges();
            roleService.LoadSchemas();

            return Ok("复制成功");
        }

        [ApiShouldAuthorize("删除角色")]
        public ActionResult RemoveRoleSchema()
        {
            Guid id = Get("id", Guid.Empty);

            var (success, msg) = roleService.RemoveRoleSchema(id, Client);

            return Ok(new
            {
                success,
                msg
            });
        }

        private class Submit
        {
#pragma warning disable IDE1006 // 命名样式
            public Guid id { get; set; } = Guid.Empty;
            public List<AuthorizeDescriptor> permits { get; set; } = [];
            public List<AuthorizeDescriptor> menus { get; set; } = [];
#pragma warning restore IDE1006 // 命名样式
        }
        #endregion
    }
}
