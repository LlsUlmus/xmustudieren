namespace Ricebird.Cms.Models
{
    public class AttachmentRepository(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<CmsAttachment>(ctx, scoped)
    {
    }
}
