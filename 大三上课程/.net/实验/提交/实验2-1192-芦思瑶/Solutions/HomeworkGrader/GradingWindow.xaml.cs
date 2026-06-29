using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HomeworkGrader.ViewModels;

namespace HomeworkGrader
{
    /// <summary>
    /// GradingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GradingWindow : Window
    {
        public GradingWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<GradingViewModel>();
        }

        public GradingWindow(int assignmentId) : this()
        {
            // assignmentId 可用于初始化 ViewModel
        }
    }
}

