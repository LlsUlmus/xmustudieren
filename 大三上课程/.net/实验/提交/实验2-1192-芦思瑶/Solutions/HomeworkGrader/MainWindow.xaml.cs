using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HomeworkGrader.ViewModels;

namespace HomeworkGrader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<MainViewModel>();
        }
    }
}

