using Ricebird.Framework.DataValidator;

namespace Ricebird.Security.Services
{
    public class MenuService(IServiceProvider sp) : ISingletonDependency
    {
        private IServiceProvider ServiceProvider { get; set; } = sp;

        private readonly object menuLck = new object();
        private List<MenuItem> _allMenus = [];

        public List<MenuItem> AllMenus
        {
            get
            {
                if (_allMenus.Count == 0)
                {
                    LoadAllMenus();
                }
                return _allMenus;
            }
        }

        internal void LoadAllMenus()
        {
            lock (menuLck)
            {
                using var scoped = ServiceProvider.CreateScope();
                var menuRepo = scoped.Resolve<MenuRepository>();
                var newMenus = menuRepo.LoadAllNodes();
                _allMenus.Clear();
                _allMenus = newMenus;
            }
        }

        public MenuItem? GetMenuItemById(Guid id) => AllMenus.FirstOrDefault(e => e.ID == id);

        public void ReorderMenu(IClient client)
        {
            var menuRepo = client.Resolve<MenuRepository>();
            var newMenus = menuRepo.LoadAllNodes();
            int i = 100;
            foreach (var lv1 in newMenus.Where(e => e.ParentId == Guid.Empty))
            {
                lv1.DisplayOrder = i;
                i += 100;
                int j = 100;
                foreach (var lv2 in lv1.Children)
                {
                    lv2.DisplayOrder = j;
                    j += 100;
                }
            }
            menuRepo.SaveChanges();

            _allMenus.Clear();
            _allMenus = newMenus;
        }

        public (bool success, string msg, object data) GetMenus(Guid parentId, bool force)
        {
            if (force || _allMenus.Count == 0)
            {
                LoadAllMenus();
            }

            if (parentId != Guid.Empty)
            {
                var dpts = _allMenus.Where(e => e.ParentId == parentId).ToList();
                var data = InternalMenusToTreeData(dpts);
                return (true, "", data);
            }

            var all = InternalMenusToTreeData(_allMenus.Where(e => e.ParentId == Guid.Empty).ToList());
            return (true, "", all);
        }

        public (bool success, string msg, ValidateResult result, MenuItem? data) SaveMenus(IClient client, bool force)
        {
            MenuRepository repo = client.Resolve<MenuRepository>();
            var (_, entity) = repo.FillEntity(client);

            var result = entity.Validate(client);
            if (!force && !result)
            {
                return (false, "", result, null);
            }

            repo.SaveChanges();

            return (true, "", result, entity);
        }

        public (bool success, string msg) RemoveMenu(Guid id, IClient client)
        {
            var menu = GetMenuItemById(id);
            if (menu == null)
            {
                return (true, "");
            }

            List<Guid> ids = menu.AllChildren.Select(e => e.ID).ToList();
            ids.Add(id);

            MenuRepository repo = client.Resolve<MenuRepository>();
            repo.RemoveWhere(e => ids.Contains(e.ID), true);

            LoadAllMenus();

            return (true, "");
        }

        protected IEnumerable<dynamic> InternalMenusToTreeData(IEnumerable<MenuItem> menus)
        {
            foreach (var e in menus)
            {
                bool isParent = (e.Children != null && e.Children.Any());

                if (isParent)
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    List<dynamic> childrenObj = [.. InternalMenusToTreeData(e.Children)];
#pragma warning restore CS8604 // 引用类型参数可能为 null。

                    var obj = new
                    {
                        e.ID,
                        e.ParentId,
                        e.Name,
                        e.Visibility,
                        e.DisplayOrder,
                        e.Icon,
                        e.LinkType,
                        e.LinkTo,
                        e.QueryString,
                        e.Parameters,
                        children = childrenObj
                    };
                    yield return obj;
                }
                else
                {
                    var obj = new
                    {
                        e.ID,
                        e.ParentId,
                        e.Name,
                        e.Visibility,
                        e.DisplayOrder,
                        e.Icon,
                        e.LinkType,
                        e.LinkTo,
                        e.QueryString,
                        e.Parameters
                    };
                    yield return obj;
                }// if
            } // foreach
        }
    }
}
