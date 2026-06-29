using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Ricebird.Framework.Excel;
using System.Data;
using System.Text.RegularExpressions;

namespace Ricebird.Framework
{
    public static partial class ExcelExtensions
    {
        public static IWorkbook? ReadAsWorkbook(this Stream stream, string ext)
        {
            var memStream = stream.CopyToMemory();
            ExcelType type = ext.EndsWith(".xlsx") ? ExcelType.Xlsx : ExcelType.Xls;
            IWorkbook? workbook;
            try
            {
                workbook = type switch
                {
                    ExcelType.Xls => new HSSFWorkbook(memStream),
                    ExcelType.Xlsx => new XSSFWorkbook(memStream),
                    _ => null,
                };
            }
            catch
            {
                workbook = null;
            }

            return workbook;
        }

        /// <summary>
        /// 将工作本转换为工作表
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <param name="titleRowIndex">标题所在行（从0开始），默认值为1，即第2行。</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IWorkbook workbook, string sheetName = "", int titleRowIndex = 1)
        {
            DataTable dt = new DataTable();
            if (workbook == null)
            {
                return dt;
            }

            int sheetIndex = 0;
            if (!string.IsNullOrWhiteSpace(sheetName))
            {
                sheetIndex = workbook.GetSheetIndex(sheetName);
            }

            ISheet sheet = workbook.GetSheetAt(sheetIndex < 0 ? 0 : sheetIndex);
            int maxRow = sheet.LastRowNum + 1;
            if (maxRow <= titleRowIndex)
            {
                return dt;
            }

            IRow titleRow = sheet.GetRow(titleRowIndex);
            //生成头
            var maxCell = titleRow.LastCellNum;
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < maxCell; i++)
            {
                int c1 = i / 26;
                int c2 = i % 26;
                string cName = c1 == 0 ? $"{alphabets[c2]}" : $"{alphabets[c1 - 1]}{alphabets[c2]}";
                dt.Columns.Add(cName);
            }
            titleRowIndex++;

            //生成表
            for (int i = titleRowIndex; i < maxRow; i++)
            {
                IRow data = sheet.GetOrCreateRow(i);
                if (string.IsNullOrWhiteSpace(data.GetOrCreateCell(0).GetCellStringValue()) && string.IsNullOrWhiteSpace(data.GetOrCreateCell(1).GetCellStringValue()))
                {
                    continue;
                }
                DataRow dr = dt.NewRow();
                for (int j = 0; j < maxCell; j++)
                {
                    dr[j] = data.GetOrCreateCell(j).GetCellStringValue();
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static DateTime ToExcelDate(this DataRow dr, int index)
        {
            try
            {
                if (int.TryParse(dr[index].ToString(), out int dt))
                {
                    return (new DateTime(1900, 1, 1).AddDays(dt - 2));
                }
                else
                {
                    return dr[index].ConvertTo(ConstKeys.MinDate);
                }
            }
            catch
            {
                return ConstKeys.MinDate;
            }
        }

        public static DateTime ToExcelDate(this DataRow dr, string columnAddress)
        {
            int index = StringToLocation($"{columnAddress}1").column;
            return ToExcelDate(dr, index);
        }

        public static MemoryStream ExportStream(this IWorkbook workbook)
        {
            NpoiMemoryStream npoiStream = new NpoiMemoryStream(false);
            workbook.Write(npoiStream);
            npoiStream.IsClose = true;
            npoiStream.Seek(0, SeekOrigin.Begin);
            return npoiStream;
        }

        public static void SaveTo(this IWorkbook workbook, string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            workbook.Write(fileStream);
        }

        public static byte[] ExportBytes(this IWorkbook workbook)
        {
            NpoiMemoryStream npoiStream = new NpoiMemoryStream(false);
            workbook.Write(npoiStream);
            npoiStream.IsClose = true;
            npoiStream.Seek(0, SeekOrigin.Begin);
            return npoiStream.ToArray();
        }

        public static IWorkbook? LoadAsWorkbook(this string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }

            Stream fStream = new FileStream(file, FileMode.Open);
            Stream excelStream = fStream.CopyToMemory();
            excelStream.SeekToBegin();
            fStream.Close();
            fStream.Dispose();

            ExcelType type = file.EndsWith(".xlsx") ? ExcelType.Xlsx : ExcelType.Xls;
            IWorkbook? workbook;

            try
            {
                workbook = type switch
                {
                    ExcelType.Xls => new HSSFWorkbook(excelStream),
                    ExcelType.Xlsx => new XSSFWorkbook(excelStream),
                    _ => null,
                };
            }
            catch
            {
                workbook = null;
            }

            return workbook;
        }

        /// <summary>
        /// 根据Excel的单元格地址，获取从0开始的坐标
        /// </summary>
        /// <param name="cell">Excel的单元格地址</param>
        /// <returns>从0开始的坐标</returns>
        public static (int column, int row) StringToLocation(string cell)
        {
            string columnStr = "";
            string rowStr = "0";
            foreach (char c in cell)
            {
                if (IsChar().IsMatch($"{c}"))
                {
                    columnStr += c;
                }
                else
                {
                    rowStr += c;
                }
            }

            int row = int.Parse(rowStr) - 1;
            columnStr = columnStr.ToUpper();

            int column = 0;
            if (columnStr.Length == 1)
            {
                column = columnStr[0] - 'A';
            }
            else if (columnStr.Length == 2)
            {
                column = (columnStr[1] - 'A' + 1) * 26 + (columnStr[0] - 'A');
            }

            return (column, row);
        }

        public static ISheet GetOrCreateSheetAt(this IWorkbook workbook, int index)
        {
            try
            {
                return workbook.GetSheetAt(index);
            }
            catch
            {
                return workbook.CreateSheet();
            }
        }

        /// <summary>
        /// 写入数据列
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowCount">从0开始</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IRow CreateDataRow(this ISheet sheet, int rowCount, IEnumerable<object> data)
        {
            return CreateDataRow(sheet, rowCount, _ => { }, data);
        }

        /// <summary>
        /// 写入数据列
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowCount">从0开始</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IRow CreateDataRow(this ISheet sheet, int rowCount, Action<IRow> rowBuilder, IEnumerable<object> data)
        {
            IRow row = sheet.GetRow(rowCount);
            row ??= sheet.CreateRow(rowCount);
            rowBuilder(row);
            int cc = 0; // 当前列            
            foreach (object d in data)
            {
                ICell cell = row.GetOrCreateCell(cc);
                cell.SetObjectValue(d);
                cc++;
            }

            return row;
        }

        /// <summary>
        /// 获取或者创建一行
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowCount">以0为基准的行号</param>
        /// <returns></returns>
        public static IRow GetOrCreateRow(this ISheet sheet, int rowCount)
        {
            IRow row = sheet.GetRow(rowCount);
            row ??= sheet.CreateRow(rowCount);

            return row;
        }

        /// <summary>
        /// 获取或创建一个单元格
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowCount">以0为基准的行号</param>
        /// <param name="cellCount">以0为基准的列号</param>
        /// <returns></returns>
        public static ICell GetOrCreateCell(this ISheet sheet, int rowCount, int cellCount)
        {
            IRow row = sheet.GetOrCreateRow(rowCount);
            ICell cell = row.GetCell(cellCount);
            cell ??= row.CreateCell(cellCount);

            return cell;
        }

        public static ICell GetOrCreateCell(this IRow row, int cellCount)
        {
            ICell cell = row.GetCell(cellCount);
            cell ??= row.CreateCell(cellCount);

            return cell;
        }

        public static string GetCellValue(this ISheet sheet, string address)
        {
            var (column, row) = StringToLocation(address);
            ICell cell = GetOrCreateCell(sheet, row, column);
            return cell.GetCellStringValue();
        }

        public static ICell GetCell(this ISheet sheet, string address)
        {
            var (column, row) = StringToLocation(address);
            ICell cell = GetOrCreateCell(sheet, row, column);
            return cell;
        }

        public static void SetCellValue(this ISheet sheet, string address, object value)
        {
            var (column, row) = StringToLocation(address);
            ICell cell = GetOrCreateCell(sheet, row, column);
            cell.SetObjectValue(value);
        }

        public static void SetRowValue(this ISheet sheet, int rIndex, params object[] values)
        {
            SetRowValue(sheet, rIndex, null, values: values);
        }

        /// <summary>
        /// 将数据插入Sheet
        /// </summary>
        /// <param name="sheet">待插入的工作表</param>
        /// <param name="rIndex">以0为基准的行号</param>
        /// <param name="style">表的样式</param>
        /// <param name="values">插入的值</param>
        public static void SetRowValue(this ISheet sheet, int rIndex, ICellStyle? style, params object[] values)
        {
            IRow row = sheet.GetOrCreateRow(rIndex);
            row.HeightInPoints = 24;
            for (int i = 0; i < values.Length; i++)
            {
                string item = values[i].ToString() ?? "";
                ICell cell = row.GetOrCreateCell(i);
                if (style != null)
                {
                    cell.CellStyle = style;
                }
                cell.SetObjectValue(item);
            }
        }

        /// <summary>
        /// 根据类型自动设置单元格的值
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="value"></param>
        public static void SetObjectValue(this ICell cell, object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    cell.SetCellValue((bool)value);
                    break;
                case TypeCode.SByte:
                case TypeCode.Byte:
                    cell.SetCellValue((byte)value);
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    cell.SetCellValue((short)value);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    cell.SetCellValue((int)value);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    cell.SetCellValue((long)value);
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    cell.SetCellValue((double)value);
                    break;
                case TypeCode.Decimal:
                    decimal d = (decimal)value;
                    cell.SetCellValue(((double)d));
                    break;
                case TypeCode.DateTime:
                    cell.SetCellValue((DateTime)value);
                    break;
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                default:
                    string strValue = value?.ToString() ?? "";
                    strValue = strValue.Length > 32766 ? strValue[..32765] : strValue;
                    cell.SetCellValue(strValue);
                    break;
            }
        }

        public static string GetCellStringValue(this ICell cell)
        {
            return cell.CellType switch
            {
                CellType.Unknown => string.Empty,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.String => cell.StringCellValue,
                CellType.Formula => cell.CellFormula,
                CellType.Blank => string.Empty,
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Error => cell.ErrorCellValue.ToString(),
                _ => string.Empty,
            };
        }

        public static DateTime GetCellDateTimeValue(this ICell cell)
        {
            if (cell.CellType != CellType.Numeric)
            {
                return new DateTime(1900, 1, 1);
            }

            return cell.DateCellValue ?? new DateTime(1900, 1, 1);
        }

        public static void AddMergedRegion(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(firstRow, lastRow, firstCol, lastCol));
        }

        [GeneratedRegex("[a-zA-Z]")]
        private static partial Regex IsChar();
    }

    public enum ExcelType
    {
        Xls,
        Xlsx
    }
}
