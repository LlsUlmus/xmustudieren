using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NovelDownloader.Services
{
    /// <summary>
    /// 小说下载服务类（封装HTTP请求、HTML解析、文件保存）
    /// 遵循MIT协议，仅用于个人学习，24小时内删除下载内容
    /// </summary>
    public class NovelDownloadService : IDisposable
    {
        // HTTP客户端（单例，避免重复创建连接）
        private readonly HttpClient _httpClient;
        // 配置参数
        public string SaveDirectory { get; }
        public int DownloadChapterCount { get; }
        public int RequestDelayMs { get; }
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 构造函数：初始化下载服务
        /// </summary>
        public NovelDownloadService(string saveDir = null, int chapterCount = 5, int delayMs = 1000)
        {
            // 初始化HTTP客户端（模拟浏览器请求头，避免被识别为爬虫）
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36"
            );
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // 10秒超时

            // 初始化保存目录（默认桌面“NovelDownloads”）
            SaveDirectory = string.IsNullOrEmpty(saveDir)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NovelDownloads")
                : saveDir;

            // 校验参数（限制1-10章，延迟≥500ms）
            DownloadChapterCount = chapterCount < 1 ? 1 : (chapterCount > 10 ? 10 : chapterCount);
            RequestDelayMs = delayMs < 500 ? 500 : delayMs;

            // 创建保存目录（若不存在）
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            IsDisposed = false;
        }

        /// <summary>
        /// 批量下载小说章节（核心异步方法）
        /// </summary>
        public async Task<int> DownloadChaptersAsync(string startChapterUrl, Action<int, string> progressCallback)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NovelDownloadService), "服务已释放，无法下载");
            if (string.IsNullOrWhiteSpace(startChapterUrl))
                throw new ArgumentException("起始章节URL不能为空", nameof(startChapterUrl));
            if (progressCallback == null)
                throw new ArgumentNullException(nameof(progressCallback), "进度回调不能为空");

            int successCount = 0;
            string currentUrl = startChapterUrl;

            try
            {
                for (int i = 0; i < DownloadChapterCount; i++)
                {
                    // 1. 获取章节HTML内容
                    string htmlContent = await _httpClient.GetStringAsync(currentUrl);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    // 2. 解析章节标题（适配笔趣阁类网站，XPath可按需调整）
                    var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='bookname']/h1");
                    string chapterTitle = titleNode?.InnerText?.Trim() ?? $"未知章节_{i + 1}";

                    // 3. 解析章节内容（去除广告文本）
                    var contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='content']");
                    if (contentNode == null)
                    {
                        progressCallback(i + 1, $"⚠️ 章节{i + 1}（{chapterTitle}）解析失败：未找到内容节点");
                        currentUrl = await GetNextChapterUrlAsync(htmlDoc, currentUrl); // 获取下一章URL
                        continue;
                    }

                    // 清理内容（去除广告、多余空行）
                    string chapterContent = contentNode.InnerText.Trim()
                        .Replace("&nbsp;", "")
                        .Replace("请记住本书首发域名：www.biqugexx.com", "")
                        .Replace("笔趣阁手机版阅读网址：m.biqugexx.com", "")
                        .Replace("本章未完，点击下一页继续阅读", "")
                        .Replace("\r\n\r\n", "\r\n");

                    // 4. 保存到TXT文件（处理非法文件名）
                    string safeTitle = SanitizeFileName(chapterTitle);
                    string txtPath = Path.Combine(SaveDirectory, $"{safeTitle}.txt");
                    await File.WriteAllTextAsync(txtPath, $"【{chapterTitle}】\n\n{chapterContent}");

                    // 5. 回调更新进度
                    successCount++;
                    progressCallback(i + 1, $"✅ 章节{i + 1}（{chapterTitle}）下载成功，路径：{txtPath}");

                    // 6. 获取下一章URL（无下一章则退出循环）
                    string nextUrl = await GetNextChapterUrlAsync(htmlDoc, currentUrl);
                    if (string.IsNullOrWhiteSpace(nextUrl))
                    {
                        progressCallback(i + 1, "ℹ️ 已到达最后一章，停止下载");
                        break;
                    }
                    currentUrl = nextUrl;

                    // 7. 请求延迟（避免高频请求攻击网站）
                    await Task.Delay(RequestDelayMs);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP请求错误：{ex.Message}（检查URL或网络）", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"文件保存错误：{ex.Message}（检查目录权限）", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"下载异常：{ex.Message}", ex);
            }

            return successCount;
        }

        /// <summary>
        /// 获取下一章URL（修复异步方法警告：添加await Task.CompletedTask）
        /// </summary>
        private async Task<string> GetNextChapterUrlAsync(HtmlDocument htmlDoc, string currentUrl)
        {
            // 解析“下一章”链接（XPath：包含“下一章”文本的<a>标签）
            var nextNode = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(text(),'下一章')]");
            if (nextNode == null)
                return null;

            // 获取<a>标签的href属性（可能是相对路径）
            string relativeUrl = nextNode.GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return null;

            // 相对路径转绝对URL
            var currentUri = new Uri(currentUrl);
            string nextUrl = new Uri(currentUri, relativeUrl).ToString();

            // 关键修复：添加await，消除“缺少await”警告（异步方法必须有await）
            await Task.CompletedTask;
            return nextUrl;
        }

        /// <summary>
        /// 清理文件名（去除Windows非法字符：\ / : * ? " < > |）
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }
            return fileName;
        }

        /// <summary>
        /// 释放资源（实现IDisposable接口）
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                _httpClient?.Dispose(); // 释放HTTP客户端
            }
            IsDisposed = true;
        }

        /// <summary>
        /// 析构函数（防止忘记手动Dispose）
        /// </summary>
        ~NovelDownloadService()
        {
            Dispose(false);
        }
    }
}