using Ricebird.Framework.Database.Searcher;

namespace Ricebird.Organizations.Services
{
    public class UserSearcher(IClient client, IOrgService orgService) : AbstractPaginationSearcher<User>
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
        #endregion

        public override IQueryable<User> BuildQuery()
        {
            var repo = client.Resolve<UserRepository>();
            var query = from u in repo.DbSet
                        select u;

            query = query.WhereIf(ID, e => e.ID == ID);

            List<Guid> allIds = [];
            if (DepartId != Guid.Empty)
            {
                var dpt = orgService.GetDepartById(DepartId);
                if (dpt is Department depart)
                {
                    allIds = depart.AllChildren.Select(e => e.ID).ToList();
                    allIds.Add(depart.ID);
                }
            }

            query = (RoleId == Guid.Empty, DepartId == Guid.Empty) switch
            {
                (true, true) => query,
                (true, false) => query.Where(e => repo.UserRelationships.Any(x => allIds.Contains(x.DepartId) && x.UserId == e.ID)),
                (false, true) => query.Where(e => repo.UserRelationships.Any(x => RoleId == x.RoleId && x.UserId == e.ID)),
                (false, false) => query.Where(e => repo.UserRelationships.Any(x => allIds.Contains(x.DepartId) && RoleId == x.RoleId && x.UserId == e.ID)),
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
                                            || e.Code == Filter
                                            || e.Mobile == Filter
                                            || e.Email == Filter
                                            || e.OpenId == Filter),
                };
            }
            query = query.OrderBy(e => e.DisplayOrder).ThenBy(e => e.Code);
            return query;
        }
    }
}
