
using Ricebird.Security.Models;

namespace Ricebird.Organizations.Models
{
    public class UserRelationship : EntityBase
    {
        #region 数据库字段
        public Guid UserId
        {
            get; set;
        } = Guid.Empty;

        public Guid DepartId
        {
            get; set;
        } = Guid.Empty;

        public Guid RoleId
        {
            get; set;
        } = Guid.Empty;

        public DateTime LastUpdatedOn
        {
            get; set;
        } = DateTime.Now;
        #endregion

        public override void OnModelCreating(ModelBuilder builder)
        {
            builder.BuildForeignKey<UserRelationship>(nameof(RoleSchema), nameof(RoleId));
            builder.BuildForeignKey<UserRelationship>(nameof(User), nameof(UserId));
        }
    }
}
