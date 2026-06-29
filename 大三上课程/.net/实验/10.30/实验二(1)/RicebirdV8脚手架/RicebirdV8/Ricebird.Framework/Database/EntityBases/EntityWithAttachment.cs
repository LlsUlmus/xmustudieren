#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using Ricebird.Framework.DataValidator;

namespace Ricebird.Framework.Database
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
{
    public abstract class EntityWithAttachment<TAttachment> : EntityBase
        where TAttachment : AttachmentEntityBase, new()
    {
        public List<TAttachment> Attachments
        {
            get; set;
        } = [];
    }

    public abstract class ValidateEntityWithAttachment<TAttachment> : EntityWithAttachment<TAttachment>, IValidatable
        where TAttachment : AttachmentEntityBase, new()
    {
        public abstract FluentValidator BuildValidator();
    }
}
