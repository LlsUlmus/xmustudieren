using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Ricebird.Framework.Clients;
using Ricebird.Framework.FileStorage;
using Ricebird.Framework.SignalR;

namespace Ricebird.Framework.Scedules
{
    public class ExportSchedule(ISmsService sms, IFileStorageService fileService, IClient client, string name, HostEnv hostEnv) : RicebirdSchedule(sms, fileService, client, name, hostEnv)
    {
        public string TemplatePath
        {
            get; set;
        } = string.Empty;

        public IWorkbook Workbook
        {
            get; set;
        } = new XSSFWorkbook();

        public override string Type => "导出任务";

        public ExportSchedule ExecuteExport(Action<IWorkbook, IClient, ExportSchedule> executor)
        {
            ExecuteAsync((r, c) =>
            {
                ExportSchedule ex = r as ExportSchedule ?? throw new InvalidCastException("只有 ExportSchedule 才可以调用本函数");

                if (TemplatePath.HasValue())
                {
                    string path = HostEnv.Instance.GetAppPath(TemplatePath);
                    using FileStream stream = new FileStream(path, FileMode.Open);
                    var mem = stream.CopyToMemory();
                    Workbook = new XSSFWorkbook(mem);
                }
                else
                {
                    Workbook = new XSSFWorkbook();
                }

                executor(Workbook, c, ex);
            });

            return this;
        }

        public ExportSchedule ExecuteExport(Action<IWorkbook, IClient> executor)
        {
            return ExecuteExport((w, c, e) =>
            {
                executor(w, c);
            });
        }

        public void Completed(string log, string srcFileName)
        {
            Completed(log, Workbook, srcFileName);
        }
    }
}
