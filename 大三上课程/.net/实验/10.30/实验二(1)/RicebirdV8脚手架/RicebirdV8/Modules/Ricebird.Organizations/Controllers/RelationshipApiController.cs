using Ricebird.Security.Services;

namespace Ricebird.Organizations.Controllers
{
    [ApiGroup("组织关系管理"), Route("~/api/relationships/[action]")]
    public class RelationshipApiController(RelationshipRepository repo, RoleService roleService, IOrgService orgService) : ApiController
    {
        [ApiLinkTo("获取组织关系", "/manage/security/relationships")]
        public ActionResult GetRelationships()
        {
            RelationshipSearcher searcher = Client.FillResolveObject<RelationshipSearcher>();
            var (totalRow, page, pageSize, data) = searcher.BuildPaginationData();

            return Ok(new
            {
                success = true,
                msg = "",
                totalRow,
                page,
                pageSize,
                data
            });
        }

        [ApiLinkTo("删除组织关系", "保存用户")]
        public ActionResult RemoveRelationship()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            repo.DbSet.Where(e => e.ID == id).ExecuteDelete();
            repo.SaveChanges();
            return Ok("删除成功");
        }

        private static readonly char[] seperator = [',', '，'];
        [ApiLinkTo("添加组织关系", "保存用户")]
        public ActionResult AddRelationships()
        {
            Guid departId = Get(nameof(departId), Guid.Empty);
            Guid roleId = Get(nameof(roleId), Guid.Empty);
            string code = Get(nameof(code), string.Empty);

            var role = roleService.GetRoleSchemaById(roleId);
            if (role == null)
            {
                return Fail($"找不到{roleId}对应的角色");
            }

            if (role.For == RuleFor.User)
            {
                departId = Guid.Empty;
            }

            if (role.For == RuleFor.Department && departId == Guid.Empty)
            {
                return Fail($"设置{role.Name}时必须指定一个部门。");
            }

            if (departId != Guid.Empty && orgService.GetDepartById(departId) == null)
            {
                return Fail($"找不到{departId}对应的部门");
            }

            string[] codes = code.Split(seperator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            List<Guid> userIds = repo.Users
                .Where(e => codes.Contains(e.Code) && !repo.DbSet.Any(x => x.UserId == e.ID && x.RoleId == roleId && x.DepartId == departId))
                .Select(e => e.ID)
                .ToList();

            foreach (var id in userIds)
            {
                UserRelationship relationship = new UserRelationship()
                {
                    UserId = id,
                    RoleId = roleId,
                    DepartId = departId,
                };
                repo.DbSet.Add(relationship);
            }
            repo.SaveChanges();

            return Ok("删除成功");
        }
    }
}
