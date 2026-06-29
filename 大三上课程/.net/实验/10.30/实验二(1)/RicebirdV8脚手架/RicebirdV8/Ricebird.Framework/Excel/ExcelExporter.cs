using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ricebird.Framework.Excel
{
    public class ExcelExporter
    {
        public IWorkbook ExportExcel<T>(List<T> dataset, string sheetName = "", ExcelType type = ExcelType.Xlsx)
        {
            //处理数据头
            Type t = typeof(T);
            List<ExcelColumnAttribute> propTable = new List<ExcelColumnAttribute>();
            foreach (var prop in t.GetProperties())
            {
                var eca = prop.GetCustomAttribute<ExcelColumnAttribute>(true);
                if (eca == null)
                {
                    continue;
                }

                eca.Property = prop;
                propTable.Add(eca);
            }

            propTable = propTable.OrderBy(e => e.DisplayOrder).ToList();

            IWorkbook workbook;
            switch (type)
            {
                case ExcelType.Xls:
                    workbook = new HSSFWorkbook();
                    break;
                case ExcelType.Xlsx:
                default:
                    workbook = new XSSFWorkbook();
                    break;
            }

            string sn = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName;
            ISheet sheet = workbook.CreateSheet(sn);
            //生成数据头
            int rowCount = 0;
            List<string> th = propTable.Select(e => e.ColumnName).ToList();
            sheet.CreateDataRow(rowCount, th);
            rowCount++;

            //生成数据项
            foreach (var dr in dataset)
            {
                List<string> row = propTable.Select(e =>
                {
                    if (e.ToStringMethod == null || e.Property == null) return string.Empty;
                    return e.ToStringMethod(e.Property.GetValue(dr) ?? string.Empty);
                }).ToList();
                sheet.CreateDataRow(rowCount, row);
                rowCount++;
            }

            return workbook;
        }

        public IWorkbook CreateExcel(ExcelType type = ExcelType.Xlsx)
        {
            IWorkbook workbook;
            switch (type)
            {
                case ExcelType.Xls:
                    workbook = new HSSFWorkbook();
                    break;
                case ExcelType.Xlsx:
                default:
                    workbook = new XSSFWorkbook();
                    break;
            }

            return workbook;
        }
    }
}
