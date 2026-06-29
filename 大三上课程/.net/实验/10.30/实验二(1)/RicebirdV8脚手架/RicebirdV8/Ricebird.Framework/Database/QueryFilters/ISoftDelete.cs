namespace Ricebird.Framework.Database
{
    public interface ISoftDelete
    {
        bool IsDeleted
        {
            get; set;
        }
    }
}
