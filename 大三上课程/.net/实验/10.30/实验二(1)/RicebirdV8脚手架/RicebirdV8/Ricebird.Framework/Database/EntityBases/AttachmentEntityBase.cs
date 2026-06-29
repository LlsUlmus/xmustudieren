using Ricebird.Framework.FileStorage;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Ricebird.Framework.Database
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
{
    public abstract class AttachmentEntityBase : EntityBase
    {
        [JsonIgnore]
        public override Guid ID
        {
            get;
            set;
        } = SequentialGuid.NewSuid();

        /// <summary>
        /// 附件的ID
        /// </summary>
        [JsonIgnore]
        public Guid FileId
        {
            get; set;
        } = Guid.Empty;

        [ForeignKey(nameof(FileId)), JsonIgnore]
        public PermanentFile File
        {
            get; set;
        } = new PermanentFile();

        /// <summary>
        /// 附件的用途
        /// 写附件用在哪个字段，中文英文均可。用以让前端知道这个附件要显示在哪里
        /// </summary>
        public string Usage
        {
            get; set;
        } = string.Empty;

        [NotMapped]
        public string DownloadPath => File.DownloadPath;

        [NotMapped]
        public string UniqueCode => File.UniqueCode;

        [NotMapped]
        public string DisplayName => File.DisplayName;

        [NotMapped]
        public DateTime CreatedOn => File.CreatedOn;

        public abstract void SetForeingData(EntityBase data);
    }
}
