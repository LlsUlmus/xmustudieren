using Ricebird.Framework.Database.Searcher;
using Ricebird.Organizations.ViewModels;

namespace Ricebird.Organizations.Services
{
    public class RelationshipSearcher(IClient client, IOrgService orgService) : AbstractPaginationSearcher<RelationshipViewModel>
    {
        #region 字段
        public Guid DepartId
        {
            get; set;
        } = Guid.Empty;

        public string Filter
        {
            get; set;
        } = string.Empty;

        public string FilterType
        {
            get; set;
        } = string.Empty;

        public Guid RoleId
        {
            get; set;
        } = Guid.Empty;

        public bool IncludeChildren
        {
            get; set;
        } = false;

        public string DepartType
        {
            get; set;
        } = "全部门通用";
        #endregion
        public override IQueryable<RelationshipViewModel> BuildQuery()
        {
            if (DepartId == Guid.Empty && DepartType == "未设置关系")
            {
                return FindNoDepart();
            }

            RelationshipRepository repo = client.Resolve<RelationshipRepository>();
            var query = from relation in repo.DbSet
                        join role in repo.RoleSchemas on relation.RoleId equals role.ID
                        join user in repo.Users on relation.UserId equals user.ID
                        join depart in repo.Departments on relation.DepartId equals depart.ID into grouping
                        from depart in grouping.DefaultIfEmpty()
                        orderby depart.DisplayOrder, role.DisplayOrder, relation.ID
                        select new RelationshipViewModel()
                        {
                            ID = relation.ID,
                            UserId = user.ID,
                            RealName = user.RealName,
                            Code = user.Code,
                            DepartId = relation.DepartId,
                            DepartName = relation.DepartId == Guid.Empty ? "所有部门" : depart.Name,
                            RoleId = role.ID,
                            RoleName = role.Name,
                        };

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                Filter = Filter.Trim();
                query = FilterType.ToLower() switch
                {
                    "code" => query.Where(e => e.Code == Filter),
                    "name" => query.Where(e => e.RealName.StartsWith(Filter)),
                    "equals" => query.Where(e => e.RealName == Filter),
                    _ => query.Where(e => e.RealName.StartsWith(Filter)
                                            || e.Code == Filter),
                };
            }

            query = query.WhereIf(RoleId, e => e.RoleId == RoleId);

            if (IncludeChildren)
            {
                if (DepartId != Guid.Empty)
                {
                    List<Guid> allIds = [];
                    var dpt = orgService.GetDepartById(DepartId);
                    if (dpt is Department depart)
                    {
                        allIds = depart.AllChildren.Select(e => e.ID).ToList();
                        allIds.Add(depart.ID);
                    }
                    query = query.Where(e => allIds.Contains(e.DepartId));
                }
                else
                {
                    query = query.Where(e => e.DepartId == DepartId);
                }
            }
            else
            {
                query = query.Where(e => e.DepartId == DepartId);
            }


            return query;
        }

        public IQueryable<RelationshipViewModel> FindNoDepart()
        {
            RelationshipRepository repo = client.Resolve<RelationshipRepository>();
            var query = from user in repo.Users
                        where !repo.DbSet.Any(x => x.UserId == user.ID)
                        select new RelationshipViewModel()
                        {
                            UserId = user.ID,
                            RealName = user.RealName,
                            Code = user.Code,
                            DepartName = "未设置部门",
                            RoleName = "未设置角色",
                        };

            query = FilterType.ToLower() switch
            {
                "code" => query.Where(e => e.Code == Filter),
                "name" => query.Where(e => e.RealName.StartsWith(Filter)),
                "equals" => query.Where(e => e.RealName == Filter),
                _ => query.Where(e => e.RealName.StartsWith(Filter)
                                        || e.Code == Filter),
            };

            return query;
        }
    }
}
