using System.Windows;
using System.Windows.Input;
using HomeworkGrader.Models;
using HomeworkGrader.Services;

namespace HomeworkGrader.ViewModels
{
    /// <summary>
    /// 统计ViewModel
    /// </summary>
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly StatisticsService _statisticsService;
        private readonly AssignmentService _assignmentService;

        public StatisticsViewModel(StatisticsService statisticsService, AssignmentService assignmentService)
        {
            _statisticsService = statisticsService;
            _assignmentService = assignmentService;

            LoadStatisticsCommand = new RelayCommand(async () => await LoadStatisticsAsync());
            RefreshCommand = new RelayCommand(async () => await LoadStatisticsAsync());
        }

        #region 属性

        private List<string> _courseNames = new();
        public List<string> CourseNames
        {
            get => _courseNames;
            set => SetProperty(ref _courseNames, value);
        }

        private List<string> _classNames = new();
        public List<string> ClassNames
        {
            get => _classNames;
            set => SetProperty(ref _classNames, value);
        }

        private string _selectedCourse = string.Empty;
        public string SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                SetProperty(ref _selectedCourse, value);
                LoadStatisticsCommand.Execute(null);
            }
        }

        private string _selectedClass = string.Empty;
        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                SetProperty(ref _selectedClass, value);
                LoadStatisticsCommand.Execute(null);
            }
        }

        private CourseStatistics? _statistics;
        public CourseStatistics? Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        private Dictionary<string, int> _gradeDistribution = new();
        public Dictionary<string, int> GradeDistribution
        {
            get => _gradeDistribution;
            set => SetProperty(ref _gradeDistribution, value);
        }

        private string _statusMessage = "就绪";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region 命令

        public ICommand LoadStatisticsCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region 方法

        private async Task LoadStatisticsAsync()
        {
            try
            {
                StatusMessage = "正在加载统计信息...";

                // 加载课程和班级列表
                CourseNames = await _assignmentService.GetCourseNamesAsync();
                ClassNames = await _assignmentService.GetClassNamesAsync();

                if (!string.IsNullOrEmpty(SelectedCourse) && !string.IsNullOrEmpty(SelectedClass))
                {
                    // 加载统计信息
                    Statistics = await _statisticsService.GetCourseStatisticsAsync(SelectedCourse, SelectedClass);
                    GradeDistribution = await _statisticsService.GetGradeDistributionAsync(SelectedCourse, SelectedClass);
                }

                StatusMessage = "统计信息加载完成";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载统计信息时出错: {ex.Message}";
            }
        }

        #endregion
    }
}

