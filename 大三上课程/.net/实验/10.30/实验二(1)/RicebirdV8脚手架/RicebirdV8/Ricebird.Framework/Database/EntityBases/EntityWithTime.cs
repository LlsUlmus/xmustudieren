namespace Ricebird.Framework.Database
{
    public abstract class EntityWithTime : EntityBase
    {
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime UpdatedOn { get; set; } = DateTime.Now;
    }
}
