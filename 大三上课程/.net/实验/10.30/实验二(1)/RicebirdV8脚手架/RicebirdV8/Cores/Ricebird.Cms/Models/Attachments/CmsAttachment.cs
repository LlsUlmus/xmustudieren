using System.Text.Json.Serialization;

namespace Ricebird.Cms.Models
{
    public class CmsAttachment : AttachmentEntityBase
    {
        #region 数据库字段
        public Guid ArticleId
        {
            get; set;
        }

        [ForeignKey("ArticleId"), JsonIgnore]
        public Article? Article
        {
            get; set;
        } = null;
        #endregion

        #region 非数据库字段
        public string TypeName => nameof(CmsAttachment);

        public override void SetForeingData(EntityBase data)
        {
            if (data is Article item)
            {
                Article = item;
                ArticleId = item.ID;
            }
        }
        #endregion
    }
}
