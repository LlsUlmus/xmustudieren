using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HomeworkGrader.Models;
using HomeworkGrader.Services;

namespace HomeworkGrader.ViewModels
{
    /// <summary>
    /// 作业管理ViewModel
    /// </summary>
    public class AssignmentViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;

        public AssignmentViewModel(AssignmentService assignmentService)
        {
            _assignmentService = assignmentService;

            SaveCommand = new RelayCommand(async () => await SaveAsync());
            CancelCommand = new RelayCommand(Cancel);
        }

        #region 属性

        private Assignment _assignment = new();
        public Assignment Assignment
        {
            get => _assignment;
            set => SetProperty(ref _assignment, value);
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _title = "新建作业";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        #endregion

        #region 命令

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region 方法

        public void SetAssignment(Assignment? assignment)
        {
            if (assignment != null)
            {
                Assignment = assignment;
                IsEditMode = true;
                Title = "编辑作业";
            }
            else
            {
                Assignment = new Assignment();
                IsEditMode = false;
                Title = "新建作业";
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Assignment.Title))
                {
                    MessageBox.Show("请输入作业标题", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Assignment.CourseName))
                {
                    MessageBox.Show("请输入课程名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Assignment.ClassName))
                {
                    MessageBox.Show("请输入班级名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Assignment.DueDate <= DateTime.Now)
                {
                    MessageBox.Show("截止日期必须晚于当前时间", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success;
                if (IsEditMode)
                {
                    success = await _assignmentService.UpdateAssignmentAsync(Assignment);
                }
                else
                {
                    await _assignmentService.CreateAssignmentAsync(Assignment);
                    success = true;
                }

                if (success)
                {
                    MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                else
                {
                    MessageBox.Show("保存失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        #endregion
    }
}

