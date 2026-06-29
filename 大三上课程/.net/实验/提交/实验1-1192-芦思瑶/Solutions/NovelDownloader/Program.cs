using System;
using System.Windows.Forms;

namespace NovelDownloader
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // 明确指定使用 System.Windows.Forms.Application
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Form1());
        }
    }
}