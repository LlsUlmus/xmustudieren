using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Framework.FileStorage
{
    [Index(nameof(RelateId))]
    public abstract class AttachmentBase : EntityBase
    {
        #region 数据库字段
        public Guid RelateId
        {
            get; set;
        } = Guid.Empty;

        [MaxLength(20)]
        public string Usage
        {
            get; set;
        } = string.Empty;

        [MaxLength(10)]
        public string RelateTable
        {
            get; set;
        } = string.Empty;

        public Guid FileId
        {
            get; set;
        } = Guid.Empty;
        #endregion

        [NotMapped]
        public abstract string TypeName
        {
            get;
        }

        public override void OnModelCreating(ModelBuilder builder)
        {
            Type t = this.GetType();

            builder.Entity(t)
                .HasOneByType("PermanentFile")
                .WithMany()
                .HasForeignKey("FileId");
        }
    }
}
