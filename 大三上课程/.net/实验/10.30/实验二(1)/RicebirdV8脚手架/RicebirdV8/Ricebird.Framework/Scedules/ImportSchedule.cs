using NPOI.SS.UserModel;
using Ricebird.Framework.Clients;
using Ricebird.Framework.FileStorage;
using Ricebird.Framework.SignalR;

namespace Ricebird.Framework.Scedules
{
    public class ImportSchedule : RicebirdSchedule
    {
        public ImportSchedule(ISmsService sms, IFileStorageService fileService, IClient client, string name, HostEnv hostEnv)
            : base(sms, fileService, client, name, hostEnv)
        {
            IFormFileCollection? files = client.Request?.Form.Files;
            if (files == null || files.Count == 0)
            {
                throw new FileNotFoundException("使用这种任务时，必须上传至少一个文件");
            }

            IFormFile file = files[0];
            var stream = file.OpenReadStream();
            Workbook = stream.ReadAsWorkbook(file.FileName);
        }

        public IWorkbook? Workbook
        {
            get; set;
        }

        public string BackupDirectory
        {
            get; set;
        } = string.Empty;

        public string BackupFileName
        {
            get; private set;
        } = string.Empty;

        public override string Type => "导入任务";

        public ImportSchedule ExecuteImport(Action<IWorkbook, IClient, ImportSchedule> executor)
        {
            ExecuteAsync((r, c) =>
            {
                ImportSchedule ex = r as ImportSchedule ?? throw new InvalidCastException("只有 ImportSchedule 才可以调用本函数");

                if (ex.Workbook == null)
                {
                    Cancel("上传的文件无法正确的转换为Excel，所以任务结束");
                }
                else
                {

                    if (BackupDirectory.HasValue())
                    {
                        // 如果有值，则需要备份
                        Directory.CreateDirectory(BackupDirectory);
                        BackupFileName = Path.Combine(BackupDirectory, $"{DateTime.Now:yyyyMMddHHmmssfff}.xlsx");
                        BackupFileName = BackupFileName.Replace('\\', '/');
                        ex.Workbook.SaveTo(BackupFileName);
                    }

                    executor(ex.Workbook, c, ex);
                }
            });

            return this;
        }

        public ImportSchedule ExecuteImport(Action<IWorkbook, IClient> executor)
        {
            return ExecuteImport((w, c, e) =>
            {
                executor(w, c);
            });
        }

        public void SendBackupInformation()
        {
            LogReport($"已经备份至“{Path.GetFileName(BackupFileName)}”", 0, 1);
        }
    }
}
