using System.Windows;
using HomeworkGrader.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HomeworkGrader
{
    /// <summary>
    /// StatisticsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<StatisticsViewModel>();
        }
    }
}

