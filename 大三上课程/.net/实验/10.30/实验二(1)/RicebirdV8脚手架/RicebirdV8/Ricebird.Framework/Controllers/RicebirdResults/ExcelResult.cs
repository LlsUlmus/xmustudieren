using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Ricebird.Framework.Mvc.RicebirdResult
{
    public class ExcelResult(IWorkbook workbook, string workbookName) : ActionResult
    {
        public IWorkbook Workbook
        {
            get; set;
        } = workbook;

        public string Name
        {
            get; set;
        } = workbookName;

        public override void ExecuteResult(ActionContext context)
        {
            if (Workbook == null)
            {
                throw new NullReferenceException("参数Workbook不可以空");
            }

            HttpResponse response = context.HttpContext.Response;
            // 设置 HTTP Header 
            response.ContentType = "application/vnd.ms-excel";

            string fileName = Name;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                string ext = ".xlsx";
                if (Workbook is HSSFWorkbook)
                {
                    ext = ".xls";
                }
                fileName = Guid.NewGuid().ToString("N") + ext;
            }
            response.Headers.Append("Content-Disposition", "attachment; filename=\"" + fileName + "\"");

            // 将图片数据写入Response 
            var stream = Workbook.ExportStream();
            byte[] b = stream.ToArray();
            response.BodyWriter.WriteAsync(b).GetAwaiter().GetResult();
        }
    }
}
