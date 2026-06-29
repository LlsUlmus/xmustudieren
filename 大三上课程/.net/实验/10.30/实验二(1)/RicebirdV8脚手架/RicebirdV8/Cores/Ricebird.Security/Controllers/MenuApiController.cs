namespace Ricebird.Security.Controllers
{
    [Route("~/api/menu/[action]"), ApiGroup("菜单管理")]
    public class MenuApiController(MenuService ms, RoleService rService) : ApiController
    {
        internal MenuService MenuService { get; set; } = ms;

        #region 菜单的增删查改
        [ApiLinkTo("获取菜单列表", "/manage/security/menus")]
        public ActionResult GetMenuItems()
        {
            var (success, msg, data) = MenuService.GetMenus(Guid.Empty, false);

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiLinkTo("获取菜单项", "/manage/security/menus")]
        public ActionResult GetMenu()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            MenuItem data = MenuService.GetMenuItemById(id) ?? new MenuItem()
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

        [ApiShouldAuthorize("保存菜单")]
        public ActionResult SaveMenu()
        {
            var (success, msg, result, data) = MenuService.SaveMenus(Client, false);

            if (!result)
            {
                return ValidateError(result);
            }

            if (data == null)
            {
                return Ok(new
                {
                    success,
                    msg,
                    data
                });
            }

            MenuService.LoadAllMenus();
            data = MenuService.GetMenuItemById(data.ID);
            rService._super = null;

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiShouldAuthorize("自动构建菜单")]
        public ActionResult BuildMenu()
        {
            string name = Get("display", string.Empty);
            string path = Get("path", string.Empty);
            string icon = Get("icon", string.Empty);
            LinkType type = Get("linkType", LinkType.VuePage);
            Guid parentId = Get("parentId", Guid.Empty);
            int displayOrder = Get("displayOrder", 100);

            MenuRepository repo = Resolve<MenuRepository>();
            var entity = repo.DbSet.FirstOrDefault(e => e.ParentId == parentId && e.Name == name);
            if (entity != null)
            {
                entity.ParentId = parentId;
            }
            else
            {
                entity = new MenuItem()
                {
                    Name = name,
                    LinkTo = path,
                    Icon = icon,
                    LinkType = type,
                    ParentId = parentId,
                    DisplayOrder = displayOrder,
                };
                repo.DbSet.Add(entity);
            }
            repo.SaveChanges();

            return Ok(new
            {
                success = true,
                msg = "",
                data = entity
            });
        }

        [ApiLinkTo("重新加载菜单", "自动构建菜单")]
        public ActionResult ReloadMenu()
        {
            MenuService.LoadAllMenus();
            rService._super = null;
            return Ok();
        }

        [ApiShouldAuthorize("删除菜单")]
        public ActionResult RemoveMenu()
        {
            Guid id = Get("id", Guid.Empty);

            var (success, msg) = MenuService.RemoveMenu(id, Client);
            rService._super = null;

            return Ok(new
            {
                success,
                msg
            });
        }

        [ApiShouldAuthorize("重新排列菜单")]
        public ActionResult ReorderMenu()
        {
            MenuService.ReorderMenu(Client);
            rService._super = null;
            return Ok();
        }
        #endregion

    }
}
