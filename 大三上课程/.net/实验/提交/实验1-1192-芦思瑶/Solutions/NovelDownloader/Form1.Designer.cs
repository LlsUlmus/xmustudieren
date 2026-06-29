namespace NovelDownloader
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtChapterUrl = new System.Windows.Forms.TextBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            // 关键修改：将TextBox改为RichTextBox（支持SelectionColor）
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtChapterUrl（URL输入框，保持TextBox）
            // 
            this.txtChapterUrl.Location = new System.Drawing.Point(12, 35);
            this.txtChapterUrl.Name = "txtChapterUrl";
            this.txtChapterUrl.Size = new System.Drawing.Size(600, 25);
            this.txtChapterUrl.TabIndex = 0;
            this.txtChapterUrl.PlaceholderText = "请输入起始章节URL（示例：https://www.biqugexx.com/123/123456/）";
            // 
            // btnDownload（下载按钮）
            // 
            this.btnDownload.Location = new System.Drawing.Point(618, 33);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(120, 29);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "开始下载（5章）";
            this.btnDownload.UseVisualStyleBackColor = true;
            // 
            // progressBar1（进度条）
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 70);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(820, 23);
            this.progressBar1.TabIndex = 2;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 5; // 默认下载5章
            // 
            // txtLog（关键修改：RichTextBox替代TextBox）
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 100);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(820, 400);
            this.txtLog.TabIndex = 3;
            this.txtLog.Text = "";
            this.txtLog.ReadOnly = true; // 只读，禁止用户编辑
            this.txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical; // 垂直滚动条
            this.txtLog.WordWrap = true; // 自动换行
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F); // 等宽字体，便于日志阅读
            // 
            // btnOpenDir（打开保存目录按钮）
            // 
            this.btnOpenDir.Location = new System.Drawing.Point(744, 33);
            this.btnOpenDir.Name = "btnOpenDir";
            this.btnOpenDir.Size = new System.Drawing.Size(88, 29);
            this.btnOpenDir.TabIndex = 4;
            this.btnOpenDir.Text = "打开保存目录";
            this.btnOpenDir.UseVisualStyleBackColor = true;
            // 
            // btnClearLog（清空日志按钮）
            // 
            this.btnClearLog.Location = new System.Drawing.Point(744, 70);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(88, 23);
            this.btnClearLog.TabIndex = 5;
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.UseVisualStyleBackColor = true;
            // 
            // label1（URL提示标签）
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "起始章节URL：";
            // 
            // Form1（主窗体）
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 512);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.btnOpenDir);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.txtChapterUrl);
            this.Name = "Form1";
            this.Text = "小说下载器（.NET版）- 仅用于学习";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle; // 固定窗体大小
            this.MaximizeBox = false; // 禁止最大化
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        // 控件声明（关键修改：将TextBox改为RichTextBox）
        private System.Windows.Forms.TextBox txtChapterUrl;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox txtLog; // 已改为RichTextBox
        private System.Windows.Forms.Button btnOpenDir;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Label label1;
    }
}