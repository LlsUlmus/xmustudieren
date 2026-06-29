using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NovelDownloader.Services;

namespace NovelDownloader
{
    public partial class Form1 : Form
    {
        private readonly NovelDownloadService _downloadService;

        public Form1()
        {
            InitializeComponent();
            // 初始化下载服务（默认5章，1000ms请求延迟）
            _downloadService = new NovelDownloadService(chapterCount: 5, delayMs: 1000);
            // 绑定按钮事件
            btnDownload.Click += BtnDownload_Click;
            btnOpenDir.Click += BtnOpenDir_Click;
            btnClearLog.Click += BtnClearLog_Click;
            this.FormClosing += Form1_FormClosing;
        }

        /// <summary>
        /// 开始下载按钮（异步执行，避免UI卡死）
        /// </summary>
        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            // 输入校验
            if (string.IsNullOrWhiteSpace(txtChapterUrl.Text))
            {
                ShowLog("❌ 错误：请输入起始章节URL（如笔趣阁章节链接）", isError: true);
                return;
            }

            // 禁用按钮，防止重复点击
            btnDownload.Enabled = false;
            progressBar1.Value = 0;
            ShowLog("ℹ️ 开始下载...（法律提示：下载后24小时内必须删除，禁止商用）");

            try
            {
                // 异步调用下载服务，传递进度回调
                int successCount = await _downloadService.DownloadChaptersAsync(
                    startChapterUrl: txtChapterUrl.Text,
                    progressCallback: (currentStep, message) =>
                    {
                        // 跨线程更新UI（RichTextBox需Invoke确保线程安全）
                        this.Invoke(new Action(() =>
                        {
                            progressBar1.Value = currentStep;
                            ShowLog(message);
                        }));
                    }
                );

                // 下载完成提示
                ShowLog($"🎉 下载完成！共尝试下载{_downloadService.DownloadChapterCount}章，成功{successCount}章", isSuccess: true);
                ShowLog($"📂 小说保存目录：{_downloadService.SaveDirectory}");
            }
            catch (Exception ex)
            {
                ShowLog($"❌ 下载失败：{ex.Message}", isError: true);
            }
            finally
            {
                // 恢复按钮可用状态
                btnDownload.Enabled = true;
            }
        }

        /// <summary>
        /// 打开保存目录按钮
        /// </summary>
        private void BtnOpenDir_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(_downloadService.SaveDirectory))
            {
                // 调用系统资源管理器打开目录
                Process.Start("explorer.exe", _downloadService.SaveDirectory);
            }
            else
            {
                ShowLog($"❌ 保存目录不存在：{_downloadService.SaveDirectory}", isError: true);
            }
        }

        /// <summary>
        /// 清空日志按钮
        /// </summary>
        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            ShowLog("ℹ️ 日志已清空");
        }

        /// <summary>
        /// 显示带时间戳的彩色日志（适配RichTextBox的SelectionColor）
        /// </summary>
        private void ShowLog(string message, bool isSuccess = false, bool isError = false)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logLine = $"[{timeStamp}] {message}\n";

            // 设置日志颜色（RichTextBox支持SelectionColor）
            txtLog.SelectionColor = isSuccess ? System.Drawing.Color.Green :
                                    isError ? System.Drawing.Color.Red :
                                    System.Drawing.Color.Black;

            // 添加日志并滚动到最新行
            txtLog.AppendText(logLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        /// <summary>
        /// 窗体关闭时释放下载服务资源
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _downloadService?.Dispose();
        }
    }
}