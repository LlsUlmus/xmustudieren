using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Framework.FileStorage
{
    [Index(nameof(MD5), IsUnique = false)]
    public class PermanentFile : EntityBase, IFile
    {
        #region 数据库字段
        public string DisplayName
        {
            get; set;
        } = string.Empty;

        public string ModuleName
        {
            get; set;
        } = string.Empty;

        public string PhysicPath
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 校验码
        /// </summary>
        [MaxLength(32)]
        public string MD5
        {
            get; set;
        } = string.Empty;

        public string MimeType
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public int Size
        {
            get; set;
        } = 0;

        public Guid CreateBy
        {
            get; set;
        } = Guid.Empty;

        public DateTime CreatedOn
        {
            get; set;
        } = DateTime.Now;
        #endregion

        #region 非数据库字段
        /// <summary>
        /// 下载路径
        /// </summary>
        [NotMapped]
        public string DownloadPath
        {
            get
            {
                if (File.Exists(PhysicPath))
                {
                    var fileName = Path.GetFileName(PhysicPath);

                    string[] array = fileName.Split('+');
                    string actualFileName = $"{array[1]}.{array[0]}";

                    return $"/permanent/virtual/{actualFileName}";
                    // return $"/permanent/{ModuleName}/{CreatedOn.Year}/{CreatedOn.Month}/{actualFileName}";
                }
                else
                {
                    return $"/404";
                }
            }
        }

        [NotMapped]
        public string UniqueCode => ID.To62String();

        [NotMapped]
        public FileStorageType StorageType => FileStorageType.Permanent;
        #endregion
    }
}
