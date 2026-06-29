using System.IO;
using System.IO.Compression;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;
using HomeworkGrader.Models;

namespace HomeworkGrader.Services
{
    /// <summary>
    /// 文件处理服务
    /// </summary>
    public class FileService
    {
        private readonly string _uploadPath;
        private readonly string[] _supportedExtensions = { ".docx", ".pdf", ".txt", ".zip" };

        public FileService()
        {
            _uploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        /// <summary>
        /// 保存上传的文件
        /// </summary>
        public async Task<string> SaveUploadedFileAsync(Stream fileStream, string fileName, string studentId)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            if (!_supportedExtensions.Contains(extension))
            {
                throw new ArgumentException($"不支持的文件类型: {extension}");
            }

            var studentFolder = Path.Combine(_uploadPath, studentId);
            if (!Directory.Exists(studentFolder))
            {
                Directory.CreateDirectory(studentFolder);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var newFileName = $"{timestamp}_{fileName}";
            var filePath = Path.Combine(studentFolder, newFileName);

            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamWriter);
            }

            return filePath;
        }

        /// <summary>
        /// 读取文本文件内容
        /// </summary>
        public async Task<string> ReadTextFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            switch (extension)
            {
                case ".txt":
                    return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                
                case ".pdf":
                    return await ReadPdfFileAsync(filePath);
                
                case ".docx":
                    return await ReadDocxFileAsync(filePath);
                
                case ".zip":
                    return await ReadZipFileAsync(filePath);
                
                default:
                    throw new NotSupportedException($"不支持读取文件类型: {extension}");
            }
        }

        /// <summary>
        /// 读取PDF文件
        /// </summary>
        private async Task<string> ReadPdfFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var text = new StringBuilder();
                using (var reader = new PdfReader(filePath))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        text.AppendLine(iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, i));
                    }
                }
                return text.ToString();
            });
        }

        /// <summary>
        /// 读取DOCX文件
        /// </summary>
        private async Task<string> ReadDocxFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using (var document = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(filePath, false))
                {
                    var body = document.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? string.Empty;
                }
            });
        }

        /// <summary>
        /// 读取ZIP文件
        /// </summary>
        private async Task<string> ReadZipFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var content = new StringBuilder();
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.EndsWith(".txt") || entry.Name.EndsWith(".docx"))
                        {
                            content.AppendLine($"文件: {entry.Name}");
                            using (var stream = entry.Open())
                            using (var reader = new StreamReader(stream))
                            {
                                content.AppendLine(reader.ReadToEnd());
                            }
                            content.AppendLine("---");
                        }
                    }
                }
                return content.ToString();
            });
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        public long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }
            return 0;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }

    /// <summary>
    /// 导出服务
    /// </summary>
    public class ExportService
    {
        /// <summary>
        /// 导出成绩到Excel
        /// </summary>
        public async Task<string> ExportGradesToExcelAsync(List<StudentSubmission> submissions, string assignmentTitle)
        {
            return await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("成绩单");

                // 设置标题
                worksheet.Cell(1, 1).Value = assignmentTitle;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 6).Merge();

                // 设置表头
                var headers = new[] { "学号", "姓名", "提交时间", "是否迟到", "成绩", "评语" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(3, i + 1).Value = headers[i];
                    worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // 填充数据
                for (int i = 0; i < submissions.Count; i++)
                {
                    var submission = submissions[i];
                    var row = i + 4;
                    
                    worksheet.Cell(row, 1).Value = submission.StudentId;
                    worksheet.Cell(row, 2).Value = submission.StudentName;
                    worksheet.Cell(row, 3).Value = submission.SubmittedDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 4).Value = submission.IsLate ? "是" : "否";
                    worksheet.Cell(row, 5).Value = submission.Grade?.ToString() ?? "未批改";
                    worksheet.Cell(row, 6).Value = submission.Feedback;

                    // 设置迟到行的颜色
                    if (submission.IsLate)
                    {
                        worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    }
                }

                // 自动调整列宽
                worksheet.Columns().AdjustToContents();

                // 保存文件
                var fileName = $"{assignmentTitle}_成绩单_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports", fileName);
                
                var exportDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir!);
                }

                workbook.SaveAs(filePath);
                return filePath;
            });
        }

        /// <summary>
        /// 导出统计报告
        /// </summary>
        public async Task<string> ExportStatisticsReportAsync(CourseStatistics stats, Dictionary<string, int> gradeDistribution)
        {
            return await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("统计报告");

                // 基本信息
                worksheet.Cell(1, 1).Value = $"{stats.CourseName} - {stats.ClassName} 统计报告";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 2).Merge();

                // 统计数据
                var data = new Dictionary<string, object>
                {
                    { "总作业数", stats.TotalAssignments },
                    { "总提交数", stats.TotalSubmissions },
                    { "已批改数", stats.GradedSubmissions },
                    { "平均分", stats.AverageGrade.ToString("F2") },
                    { "最高分", stats.HighestGrade },
                    { "最低分", stats.LowestGrade },
                    { "批改率", stats.TotalSubmissions > 0 ? (stats.GradedSubmissions * 100.0 / stats.TotalSubmissions).ToString("F2") + "%" : "0%" }
                };

                int row = 3;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Key;
                    worksheet.Cell(row, 2).Value = item.Value.ToString();
                    row++;
                }

                // 成绩分布
                row += 2;
                worksheet.Cell(row, 1).Value = "成绩分布";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Range(row, 1, row, 2).Merge();

                row++;
                foreach (var distribution in gradeDistribution)
                {
                    worksheet.Cell(row, 1).Value = distribution.Key;
                    worksheet.Cell(row, 2).Value = distribution.Value;
                    row++;
                }

                // 自动调整列宽
                worksheet.Columns().AdjustToContents();

                // 保存文件
                var fileName = $"{stats.CourseName}_{stats.ClassName}_统计报告_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports", fileName);
                
                var exportDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir!);
                }

                workbook.SaveAs(filePath);
                return filePath;
            });
        }
    }
}

