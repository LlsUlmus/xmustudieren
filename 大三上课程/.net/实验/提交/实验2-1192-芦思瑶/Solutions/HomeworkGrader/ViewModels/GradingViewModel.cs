using System.Windows;
using System.Windows.Input;
using HomeworkGrader.Models;
using HomeworkGrader.Services;

namespace HomeworkGrader.ViewModels
{
    /// <summary>
    /// 批改作业ViewModel
    /// </summary>
    public class GradingViewModel : ViewModelBase
    {
        private readonly SubmissionService _submissionService;
        private readonly FileService _fileService;

        public GradingViewModel(SubmissionService submissionService, FileService fileService)
        {
            _submissionService = submissionService;
            _fileService = fileService;

            LoadSubmissionsCommand = new RelayCommand(async () => await LoadSubmissionsAsync());
            GradeSubmissionCommand = new RelayCommand<StudentSubmission>(async (submission) => await GradeSubmissionAsync(submission));
            ViewFileCommand = new RelayCommand<StudentSubmission>(ViewFile);
            PreviousCommand = new RelayCommand(PreviousSubmission);
            NextCommand = new RelayCommand(NextSubmission);
        }

        #region 属性

        private int _assignmentId;
        public int AssignmentId
        {
            get => _assignmentId;
            set => SetProperty(ref _assignmentId, value);
        }

        private List<StudentSubmission> _submissions = new();
        public List<StudentSubmission> Submissions
        {
            get => _submissions;
            set => SetProperty(ref _submissions, value);
        }

        private StudentSubmission? _currentSubmission;
        public StudentSubmission? CurrentSubmission
        {
            get => _currentSubmission;
            set => SetProperty(ref _currentSubmission, value);
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        private int _grade;
        public int Grade
        {
            get => _grade;
            set => SetProperty(ref _grade, value);
        }

        private string _feedback = string.Empty;
        public string Feedback
        {
            get => _feedback;
            set => SetProperty(ref _feedback, value);
        }

        private string _graderName = Environment.UserName;
        public string GraderName
        {
            get => _graderName;
            set => SetProperty(ref _graderName, value);
        }

        private string _fileContent = string.Empty;
        public string FileContent
        {
            get => _fileContent;
            set => SetProperty(ref _fileContent, value);
        }

        private string _statusMessage = "就绪";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region 命令

        public ICommand LoadSubmissionsCommand { get; }
        public ICommand GradeSubmissionCommand { get; }
        public ICommand ViewFileCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }

        #endregion

        #region 方法

        public void SetAssignmentId(int assignmentId)
        {
            AssignmentId = assignmentId;
            LoadSubmissionsCommand.Execute(null);
        }

        private async Task LoadSubmissionsAsync()
        {
            try
            {
                StatusMessage = "正在加载提交...";
                Submissions = await _submissionService.GetSubmissionsByAssignmentIdAsync(AssignmentId);
                
                if (Submissions.Any())
                {
                    CurrentIndex = 0;
                    CurrentSubmission = Submissions[0];
                    await LoadFileContentAsync();
                }

                StatusMessage = $"已加载 {Submissions.Count} 个提交";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载提交时出错: {ex.Message}";
            }
        }

        private async Task LoadFileContentAsync()
        {
            if (CurrentSubmission == null || string.IsNullOrEmpty(CurrentSubmission.FilePath))
            {
                FileContent = "无文件内容";
                return;
            }

            try
            {
                StatusMessage = "正在读取文件内容...";
                FileContent = await _fileService.ReadTextFileAsync(CurrentSubmission.FilePath);
                StatusMessage = "文件内容加载完成";
            }
            catch (Exception ex)
            {
                FileContent = $"读取文件时出错: {ex.Message}";
                StatusMessage = "文件读取失败";
            }
        }

        private async Task GradeSubmissionAsync(StudentSubmission? submission)
        {
            if (submission == null) return;

            try
            {
                if (Grade < 0 || Grade > 100)
                {
                    MessageBox.Show("成绩必须在0-100之间", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = await _submissionService.GradeSubmissionAsync(
                    submission.Id, Grade, Feedback, GraderName);

                if (success)
                {
                    MessageBox.Show("批改成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 重新加载数据
                    await LoadSubmissionsAsync();
                    
                    // 清空输入
                    Grade = 0;
                    Feedback = string.Empty;
                }
                else
                {
                    MessageBox.Show("批改失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批改时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewFile(StudentSubmission? submission)
        {
            if (submission == null || string.IsNullOrEmpty(submission.FilePath)) return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = submission.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PreviousSubmission()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
                CurrentSubmission = Submissions[CurrentIndex];
                await LoadFileContentAsync();
            }
        }

        private async void NextSubmission()
        {
            if (CurrentIndex < Submissions.Count - 1)
            {
                CurrentIndex++;
                CurrentSubmission = Submissions[CurrentIndex];
                await LoadFileContentAsync();
            }
        }

        #endregion
    }
}

