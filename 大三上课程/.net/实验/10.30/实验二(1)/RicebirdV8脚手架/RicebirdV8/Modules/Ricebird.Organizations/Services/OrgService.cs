using Ricebird.Framework.DataValidator;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Ricebird.Organizations.Services
{
    public class OrgService(IServiceProvider ServiceProvider) : IOrgService
    {
        private readonly object dptLck = new object();
        private List<Department> _allDepartments = [];
        public IEnumerable<IDepart> AllDepartments
        {
            get
            {
                if (_allDepartments.Count == 0)
                {
                    LoadAllDeparts();
                }
                return _allDepartments;
            }
        }

        public IDepart DefaultDepart
        {
            get;
            private set;
        } = new Department();

        private FrozenDictionary<Guid, IDepart> departToOrg = FrozenDictionary<Guid, IDepart>.Empty;
        private FrozenDictionary<Guid, string> departIdToName = FrozenDictionary<Guid, string>.Empty;
        private List<IDepart> orgs = [];

        public void LoadAllDeparts(IServiceProvider? provider = null)
        {
            lock (dptLck)
            {
                using var scoped = (provider ?? ServiceProvider).CreateScope();
                DepartmentRepository dptRepo = scoped.Resolve<DepartmentRepository>();
                _allDepartments = dptRepo.LoadAllNodes();

                DefaultDepart = _allDepartments.FirstOrDefault(e => e.IsDefault) ?? new Department();

                var specialDeparts = _allDepartments.Where(e => e.SchemaName == SPECIAL_SCHEMA).ToList();
                Dictionary<Guid, IDepart> dptToOrg = [];
                foreach (var depart in specialDeparts)
                {
                    foreach (var child in depart.AllChildren)
                    {
                        dptToOrg.MergeKey(child.ID, depart);
                    }
                }

                foreach (var depart in specialDeparts)
                {
                    dptToOrg.MergeKey(depart.ID, depart);
                }

                departToOrg = dptToOrg.ToFrozenDictionary();
                departIdToName = _allDepartments.ToFrozenDictionary(e => e.ID, e => e.Name);
                orgs = specialDeparts.OrderBy(e => e.Name).Select(e => e as IDepart).ToList();
            }
        }

        internal static void EnsureCreateDepartment(IClient client, Department department)
        {
            var dptRepo = client.Resolve<DepartmentRepository>();
            var dpt = dptRepo.FirstOrDefault(e => e.Name == department.Name && e.ParentId == department.ParentId);
            if (dpt == null)
            {
                dptRepo.DbSet.Add(department);
                dptRepo.SaveChanges();
            }
        }

        public List<IDepart> GetOrgs() => orgs;

        public IDepart? GetDepartById(Guid id) => AllDepartments.FirstOrDefault(e => e.ID == id);
        public IDepart? GetDepartByName(string name) => AllDepartments.FirstOrDefault(e => e.Name == name || e.ShortName == name);

        public string this[Guid id]
        {
            get
            {
                if (departIdToName.TryGetValue(id, out var name)) return name;
                return string.Empty;
            }
        }

        public IDepart? GetOrgByDeptId(Guid id)
        {
            if (departToOrg.TryGetValue(id, out IDepart? value))
            {
                return value;
            }

            return null;
        }

        public bool TryGetOrgByDeptId(Guid id, [NotNullWhen(true)] out IDepart? depart)
        {
            if (departToOrg.TryGetValue(id, out IDepart? value))
            {
                depart = value;
                return true;
            }

            depart = null;
            return false;
        }

        public bool TryGetOrgByName(string name, [NotNullWhen(true)] out IDepart? depart)
        {
            depart = orgs.FirstOrDefault(e => e.Name == name || e.ShortName == name);
            return depart != null;
        }

        public (bool success, string msg, object data) GetDepartTree(string categoryName, Guid parentId, bool force, IClient client)
        {
            if (force || _allDepartments.Count == 0)
            {
                LoadAllDeparts(client.Services);
            }

            if (parentId != Guid.Empty)
            {
                var dpts = _allDepartments.Where(e => e.ParentId == parentId).ToList();
                var data = InternalDepartsToTreeData(dpts);
                return (true, "", data);
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var dpts = _allDepartments.Where(e => e.SchemaName == categoryName).ToList();
                var data = InternalDepartsToTreeData(dpts);
                return (true, "", data);
            }

            var all = InternalDepartsToTreeData(_allDepartments.Where(e => e.ParentId == Guid.Empty).ToList());
            return (true, "", all);
        }

        public (bool success, string msg, ValidateResult result, IDepart? data) SaveDepartment(IClient client, bool force)
        {
            DepartmentRepository repo = client.Resolve<DepartmentRepository>();
            var (opera, entity) = repo.FillEntity(client, "Source");

            var validator = entity.BuildValidator();
            ValidateResult result = new ValidateResult(entity);
            if (!force)
            {
                result = validator.Validate(entity, client);
                if (!result)
                {
                    return (false, "", result, null);
                }
            }

            if (opera == DbOperate.Create)
            {
                entity.Source = "后台新建";
            }

            if (entity.IsDefault)
            {
                repo.DbSet.Where(e => e.ID != entity.ID).ExecuteUpdate(set => set.SetProperty(e => e.IsDefault, e => false));
                DefaultDepart = entity;
            }

            repo.SaveChanges();

            // LoadAllDeparts(client.Services);

            return (true, "", result, entity);
        }

        public void RemoveDepartment(Guid id, IClient client)
        {
            DepartmentRepository repo = client.Resolve<DepartmentRepository>();
            repo.RemoveWhere(e => e.ID == id, true);

            RelationshipRepository rr = client.Resolve<RelationshipRepository>();
            rr.ClearRelationships();

            LoadAllDeparts(client.Services);
        }

        protected IEnumerable<dynamic> InternalDepartsToTreeData(IEnumerable<Department> departs)
        {
            foreach (var e in departs)
            {
                bool isParent = (e.Children != null && e.Children.Any());

                if (isParent)
                {
                    List<dynamic> childrenObj = [.. InternalDepartsToTreeData(e.Children!)];

                    var obj = new
                    {
                        e.ID,
                        e.Name,
                        e.ParentId,
                        e.Source,
                        e.StrictCreditStrategy,
                        key = e.ID,
                        title = e.Name,
                        pid = e.ParentId,
                        label = e.ToString(),
                        e.Code,
                        e.DisplayOrder,
                        isParent,
                        e.ShortName,
                        e.SchemaName,
                        selected = false,
                        e.IsDefault,
                        children = childrenObj
                    };
                    yield return obj;
                }
                else
                {
                    var obj = new
                    {
                        e.ID,
                        e.Name,
                        e.ParentId,
                        e.Source,
                        e.StrictCreditStrategy,
                        key = e.ID,
                        title = e.Name,
                        pid = e.ParentId,
                        label = e.ToString(),
                        e.Code,
                        e.DisplayOrder,
                        isParent,
                        e.ShortName,
                        e.SchemaName,
                        e.IsDefault,
                        selected = false,
                    };
                    yield return obj;
                }// if
            } // foreach
        }

        /// <summary>
        /// 移动部门
        /// </summary>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="opera"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public (bool success, string msg, List<Department> affectRow) MoveDepartment(Guid to, List<Guid> value, string opera, IClient client)
        {
            DepartmentRepository cr = client.Resolve<DepartmentRepository>();

            var ans = cr.MoveDepartment(to, value, opera);
            if (ans.success)
            {
                RelationshipRepository rr = client.Resolve<RelationshipRepository>();
                rr.ClearRelationships();
                // LoadAllDeparts();
            }

            return ans;
        }
    }
}
