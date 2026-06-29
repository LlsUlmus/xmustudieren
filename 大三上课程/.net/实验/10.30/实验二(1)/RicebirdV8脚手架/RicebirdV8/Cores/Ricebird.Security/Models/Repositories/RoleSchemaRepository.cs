namespace Ricebird.Security.Models.Repositories
{
    internal class RoleSchemaRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<RoleSchema>(ctx, scoped)
    {
        //public void CancelDefaultRoleSchema(SequentialGuid roleSchemaId)
        //{
        //    DbSet
        //        .Where(e => e.IsDefaultRole == true)
        //        .ExecuteUpdate(set => set.SetProperty(e => e.IsDefaultRole, e => false));
        //}
    }
}
