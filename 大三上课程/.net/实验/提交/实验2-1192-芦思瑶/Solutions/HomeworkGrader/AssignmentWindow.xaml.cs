using System.Windows;
using HomeworkGrader.Models;
using HomeworkGrader.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HomeworkGrader
{
    /// <summary>
    /// AssignmentWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AssignmentWindow : Window
    {
        public AssignmentWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<AssignmentViewModel>();
        }

        public AssignmentWindow(Assignment? assignment) : this()
        {
            var viewModel = (AssignmentViewModel)DataContext;
            viewModel.SetAssignment(assignment);
        }
    }
}

