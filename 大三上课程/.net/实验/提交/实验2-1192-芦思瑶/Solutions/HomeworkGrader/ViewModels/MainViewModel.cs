using System.Windows.Controls;
using System.Windows.Input;
using HomeworkGrader.ViewModels;
using HomeworkGrader.Models;
using HomeworkGrader.Services;
using System.Windows;

namespace HomeworkGrader.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly SubmissionService _submissionService;
        private readonly StatisticsService _statisticsService;

        public MainViewModel(AssignmentService assignmentService, 
                           SubmissionService submissionService, 
                           StatisticsService statisticsService)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _statisticsService = statisticsService;

            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            CreateAssignmentCommand = new RelayCommand(CreateAssignment);
            GradeAssignmentsCommand = new RelayCommand(GradeAssignments);
            ViewStatisticsCommand = new RelayCommand(ViewStatistics);
            ExportDataCommand = new RelayCommand(ExportData);
        }

        #region 属性

        private List<Assignment> _assignments = new();
        public List<Assignment> Assignments
        {
            get => _assignments;
            set => SetProperty(ref _assignments, value);
        }

        private Assignment? _selectedAssignment;
        public Assignment? SelectedAssignment
        {
            get => _selectedAssignment;
            set => SetProperty(ref _selectedAssignment, value);
        }

        private int _totalAssignments;
        public int TotalAssignments
        {
            get => _totalAssignments;
            set => SetProperty(ref _totalAssignments, value);
        }

        private int _totalSubmissions;
        public int TotalSubmissions
        {
            get => _totalSubmissions;
            set => SetProperty(ref _totalSubmissions, value);
        }

        private int _ungradedSubmissions;
        public int UngradedSubmissions
        {
            get => _ungradedSubmissions;
            set => SetProperty(ref _ungradedSubmissions, value);
        }

        private string _statusMessage = "就绪";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region 命令

        public ICommand LoadDataCommand { get; }
        public ICommand CreateAssignmentCommand { get; }
        public ICommand GradeAssignmentsCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ExportDataCommand { get; }

        #endregion

        #region 方法

        private async Task LoadDataAsync()
        {
            try
            {
                StatusMessage = "正在加载数据...";
                
                Assignments = await _assignmentService.GetAllAssignmentsAsync();
                TotalAssignments = Assignments.Count;

                var allSubmissions = new List<StudentSubmission>();
                foreach (var assignment in Assignments)
                {
                    var submissions = await _submissionService.GetSubmissionsByAssignmentIdAsync(assignment.Id);
                    allSubmissions.AddRange(submissions);
                }

                TotalSubmissions = allSubmissions.Count;
                UngradedSubmissions = allSubmissions.Count(s => !s.IsGraded);

                StatusMessage = $"数据加载完成 - 共{TotalAssignments}个作业，{TotalSubmissions}个提交，{UngradedSubmissions}个待批改";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据时出错: {ex.Message}";
            }
        }

        private void CreateAssignment()
        {
            var assignmentWindow = new AssignmentWindow();
            assignmentWindow.ShowDialog();
            LoadDataCommand.Execute(null);
        }

        private void GradeAssignments()
        {
            if (SelectedAssignment == null)
            {
                MessageBox.Show("请先选择一个作业", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var gradingWindow = new GradingWindow(SelectedAssignment.Id);
            gradingWindow.ShowDialog();
            LoadDataCommand.Execute(null);
        }

        private void ViewStatistics()
        {
            var statisticsWindow = new StatisticsWindow();
            statisticsWindow.ShowDialog();
        }

        private void ExportData()
        {
            if (SelectedAssignment == null)
            {
                MessageBox.Show("请先选择一个作业", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 导出逻辑将在后续实现
            MessageBox.Show("导出功能开发中...", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}

