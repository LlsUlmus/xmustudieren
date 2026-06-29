using System;
using System.Windows.Forms;

namespace SnakeGame
{
    /// <summary>
    /// 程序入口类（解决"不包含适合入口点的静态Main方法"错误）
    /// </summary>
    static class Program
    {
        [STAThread] // Windows 窗体必须的线程模型
        static void Main()
        {
            Application.EnableVisualStyles(); // 启用视觉样式（美化界面）
            Application.SetCompatibleTextRenderingDefault(false); // 兼容文本渲染
            Application.Run(new Form1()); // 启动主窗体
        }
    }
}