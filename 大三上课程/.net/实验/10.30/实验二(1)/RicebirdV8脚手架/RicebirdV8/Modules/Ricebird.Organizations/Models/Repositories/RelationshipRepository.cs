
using Ricebird.Security.Models;

namespace Ricebird.Organizations.Models
{
    public class RelationshipRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<UserRelationship>(ctx, scoped)
    {
        public DbSet<User> Users => DbContext.Set<User>();
        public DbSet<Department> Departments => DbContext.Set<Department>();
        public DbSet<RoleSchema> RoleSchemas => DbContext.Set<RoleSchema>();

        public void ClearRelationships()
        {
            DbContext.Database.ExecuteSql($"""
                DELETE
                FROM UserRelationship 
                WHERE DepartId<>'00000000-0000-0000-0000-000000000000' AND NOT EXISTS(SELECT * FROM Department B WHERE DepartId=B.ID)
                """);
        }
    }
}
