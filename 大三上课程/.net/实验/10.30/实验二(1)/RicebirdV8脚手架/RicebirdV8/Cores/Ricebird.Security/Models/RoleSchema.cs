using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.DataValidator;
using Ricebird.Framework.Security.Apis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Security.Models
{
    public class RoleSchema : AscendingEntityBase, IValidatable
    {
        #region 数据库字段
        [Required, MaxLength(20)]
        public string Name
        {
            get; set;
        } = string.Empty;

        public RuleFor For
        {
            get; set;
        } = RuleFor.Department;

        /// <summary>
        /// 在适用范围为部门的情况下生效
        /// <para>
        /// 显示为：{ 部门名 }的{ DisplayAs }
        /// </para>
        /// </summary>
        [MaxLength(20)]
        public string DisplayAs
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 如果未设置权限等级，则视为
        /// <para>
        /// 此项不能设置为未设置
        /// </para>
        /// </summary>
        public AuthorizeResult NotSetEquals
        {
            get;
            set;
        } = AuthorizeResult.Deny;

        public bool IsDefaultRole
        {
            get; set;
        } = false;

        public bool CanEdit
        {
            get; set;
        } = false;

        public bool UseAsPrincipal
        {
            get; set;
        } = true;

        /// <summary>
        /// 角色菜单权限，此对象的Name属性保存的是菜单的ID
        /// </summary>
        public List<AuthorizeDescriptor> Menus
        {
            get; set;
        } = [];

        /// <summary>
        /// 角色功能权限，此对象的Name属性保存的是权限的名称
        /// </summary>
        public List<AuthorizeDescriptor> Permissions
        {
            get; set;
        } = [];
        #endregion

        #region 非数据库字段
        [NotMapped]
        public List<object> FinalMenus
        {
            get;
            internal set;
        } = [];

        [NotMapped]
        public List<string> FinalPermissions
        {
            get;
            internal set;
        } = [];
        #endregion

        public bool Successed(ApiDescriptor api)
        {
            var p = Permissions.FirstOrDefault(e => e.Name == api.Name);

            if (p != null)
            {
                return p.Result == AuthorizeResult.Access;
            }

            return NotSetEquals == AuthorizeResult.Access;
        }

        public bool Successed(string linkTo)
        {
            var p = Permissions.FirstOrDefault(e => e.Name == linkTo);

            if (p != null)
            {
                return p.Result == AuthorizeResult.Access;
            }

            return NotSetEquals == AuthorizeResult.Access;
        }

        public bool Successed(MenuItem menu)
        {
            var p = Menus.FirstOrDefault(e => e.Name == menu.ID.ToString());

            if (p != null)
            {
                return p.Result == AuthorizeResult.Access;
            }

            return NotSetEquals == AuthorizeResult.Access;
        }

        public Role ToRole(Guid forDepart, string depart, bool useAsPriciple)
        {
            string forName = For switch
            {
                RuleFor.User => Name,
                RuleFor.Department => $"{depart}的{DisplayAs}",
                RuleFor.Any when forDepart != Guid.Empty => $"{depart}的{(DisplayAs.HasValue() ? DisplayAs : Name)}",
                _ => Name
            };

            var role = new Role()
            {
                ID = ID,
                Name = Name,
                ForDepart = forDepart,
                DisplayName = forName,
                NotSetEquals = NotSetEquals,
                Menus = FinalMenus,
                DepartName = depart,
                UseAsPrinciple = UseAsPrincipal,
                Permissions = FinalPermissions
            };
            return role;
        }

        public FluentValidator BuildValidator()
        {
            FluentValidator<RoleSchema> fluent = new FluentValidator<RoleSchema>();
            fluent.AutoRulesByAttributes();

            fluent.RuleFor(e => e.Name).Should((c, r, o) =>
            {
                RoleService roleService = c.Resolve<RoleService>();
                var schema = roleService.Schemas.Where(e => e.Value.Name == o.Name).Select(e => e.Value).FirstOrDefault();
                if (schema != null)
                {
                    if (schema.ID != o.ID)
                    {
                        r.SetFailure($"名为{Name}的角色已经存在，请更换角色名");
                        return;
                    }

                    string[] notAllow = ["匿名用户", "超级管理员"];
                    if (notAllow.Any(e => schema.Name == e))
                    {
                        r.SetFailure($"不允许使用{o.Name}，这是一个内置名称。");
                        return;
                    }

                    if (!schema.CanEdit)
                    {
                        // 对于不允许修改的值，直接把值重新绑回去
                        (For, DisplayAs, NotSetEquals, IsDefaultRole, CanEdit) = (schema.For, schema.DisplayAs, schema.NotSetEquals, schema.IsDefaultRole, schema.CanEdit);
                        return;
                    }
                }

                // 对于其它情况，这两个设置默认值
                o.CanEdit = true;
                o.IsDefaultRole = false;
            });

            fluent.RuleFor(e => e.NotSetEquals).Should((r, o) =>
            {
                if (o.NotSetEquals is AuthorizeResult.NoSet)
                {
                    r.SetFailure($"不允许将此字段设置为未设置。");
                }
            });

            fluent.RuleFor(e => e.IsDefaultRole).Should((r, o) =>
            {
                if (o.IsDefaultRole)
                {
                    r.SetFailure($"不允许设置默认角色");
                }
            });

            fluent.RuleFor(e => e.Menus).Should((r, o) => Menus.RemoveAll(e => e.Result == AuthorizeResult.NoSet));

            fluent.RuleFor(e => e.Permissions).Should((r, o) => Permissions.RemoveAll(e => e.Result == AuthorizeResult.NoSet));

            return fluent;
        }

        public override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RoleSchema>().OwnsMany(e => e.Menus, navigationBuilder =>
            {
                navigationBuilder.ToJson();
            });

            builder.Entity<RoleSchema>().OwnsMany(e => e.Permissions, navigationBuilder =>
            {
                navigationBuilder.ToJson();
            });
        }

    }
}
