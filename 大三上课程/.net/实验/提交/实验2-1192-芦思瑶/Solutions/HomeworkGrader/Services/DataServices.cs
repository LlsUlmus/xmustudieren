using Microsoft.EntityFrameworkCore;
using HomeworkGrader.Models;
using HomeworkGrader.Data;

namespace HomeworkGrader.Services
{
    /// <summary>
    /// 作业管理服务
    /// </summary>
    public class AssignmentService
    {
        private readonly HomeworkGraderContext _context;

        public AssignmentService(HomeworkGraderContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取所有作业
        /// </summary>
        public async Task<List<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignments
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取作业
        /// </summary>
        public async Task<Assignment?> GetAssignmentByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// 创建新作业
        /// </summary>
        public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        /// <summary>
        /// 更新作业
        /// </summary>
        public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            try
            {
                _context.Assignments.Update(assignment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除作业
        /// </summary>
        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            try
            {
                var assignment = await _context.Assignments.FindAsync(id);
                if (assignment != null)
                {
                    assignment.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取课程列表
        /// </summary>
        public async Task<List<string>> GetCourseNamesAsync()
        {
            return await _context.Assignments
                .Where(a => a.IsActive)
                .Select(a => a.CourseName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        /// <summary>
        /// 获取班级列表
        /// </summary>
        public async Task<List<string>> GetClassNamesAsync()
        {
            return await _context.Assignments
                .Where(a => a.IsActive)
                .Select(a => a.ClassName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }

    /// <summary>
    /// 学生提交服务
    /// </summary>
    public class SubmissionService
    {
        private readonly HomeworkGraderContext _context;

        public SubmissionService(HomeworkGraderContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取作业的所有提交
        /// </summary>
        public async Task<List<StudentSubmission>> GetSubmissionsByAssignmentIdAsync(int assignmentId)
        {
            return await _context.StudentSubmissions
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }

        /// <summary>
        /// 添加学生提交
        /// </summary>
        public async Task<StudentSubmission> AddSubmissionAsync(StudentSubmission submission)
        {
            // 检查是否迟到
            var assignment = await _context.Assignments.FindAsync(submission.AssignmentId);
            if (assignment != null && submission.SubmittedDate > assignment.DueDate)
            {
                submission.IsLate = true;
            }

            _context.StudentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        /// <summary>
        /// 批改作业
        /// </summary>
        public async Task<bool> GradeSubmissionAsync(int submissionId, int grade, string feedback, string graderName)
        {
            try
            {
                var submission = await _context.StudentSubmissions.FindAsync(submissionId);
                if (submission == null) return false;

                submission.Grade = grade;
                submission.Feedback = feedback;
                submission.IsGraded = true;
                submission.GradedDate = DateTime.Now;

                // 添加批改记录
                var gradingRecord = new GradingRecord
                {
                    SubmissionId = submissionId,
                    GraderName = graderName,
                    Grade = grade,
                    Comments = feedback,
                    GradedDate = DateTime.Now
                };

                _context.GradingRecords.Add(gradingRecord);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取未批改的作业
        /// </summary>
        public async Task<List<StudentSubmission>> GetUngradedSubmissionsAsync()
        {
            return await _context.StudentSubmissions
                .Where(s => !s.IsGraded)
                .Include(s => s.Assignment)
                .OrderBy(s => s.SubmittedDate)
                .ToListAsync();
        }

        /// <summary>
        /// 批量导入学生提交
        /// </summary>
        public async Task<int> ImportSubmissionsAsync(List<StudentSubmission> submissions)
        {
            int importedCount = 0;
            foreach (var submission in submissions)
            {
                // 检查是否已存在
                var existing = await _context.StudentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == submission.AssignmentId && 
                                            s.StudentId == submission.StudentId);
                
                if (existing == null)
                {
                    _context.StudentSubmissions.Add(submission);
                    importedCount++;
                }
            }
            
            await _context.SaveChangesAsync();
            return importedCount;
        }
    }

    /// <summary>
    /// 统计服务
    /// </summary>
    public class StatisticsService
    {
        private readonly HomeworkGraderContext _context;

        public StatisticsService(HomeworkGraderContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取课程统计信息
        /// </summary>
        public async Task<CourseStatistics> GetCourseStatisticsAsync(string courseName, string className)
        {
            var stats = await _context.CourseStatistics
                .FirstOrDefaultAsync(cs => cs.CourseName == courseName && cs.ClassName == className);

            if (stats == null)
            {
                stats = await CalculateCourseStatisticsAsync(courseName, className);
            }

            return stats;
        }

        /// <summary>
        /// 计算课程统计信息
        /// </summary>
        private async Task<CourseStatistics> CalculateCourseStatisticsAsync(string courseName, string className)
        {
            var assignments = await _context.Assignments
                .Where(a => a.CourseName == courseName && a.ClassName == className && a.IsActive)
                .ToListAsync();

            var submissions = await _context.StudentSubmissions
                .Where(s => s.Assignment.CourseName == courseName && s.Assignment.ClassName == className)
                .ToListAsync();

            var gradedSubmissions = submissions.Where(s => s.IsGraded && s.Grade.HasValue).ToList();

            var stats = new CourseStatistics
            {
                CourseName = courseName,
                ClassName = className,
                TotalAssignments = assignments.Count,
                TotalSubmissions = submissions.Count,
                GradedSubmissions = gradedSubmissions.Count,
                AverageGrade = gradedSubmissions.Any() ? gradedSubmissions.Average(s => s.Grade!.Value) : 0,
                HighestGrade = gradedSubmissions.Any() ? gradedSubmissions.Max(s => s.Grade!.Value) : 0,
                LowestGrade = gradedSubmissions.Any() ? gradedSubmissions.Min(s => s.Grade!.Value) : 0,
                LastUpdated = DateTime.Now
            };

            // 保存或更新统计信息
            var existing = await _context.CourseStatistics
                .FirstOrDefaultAsync(cs => cs.CourseName == courseName && cs.ClassName == className);

            if (existing != null)
            {
                _context.CourseStatistics.Update(stats);
            }
            else
            {
                _context.CourseStatistics.Add(stats);
            }

            await _context.SaveChangesAsync();
            return stats;
        }

        /// <summary>
        /// 获取成绩分布
        /// </summary>
        public async Task<Dictionary<string, int>> GetGradeDistributionAsync(string courseName, string className)
        {
            var submissions = await _context.StudentSubmissions
                .Where(s => s.Assignment.CourseName == courseName && 
                           s.Assignment.ClassName == className && 
                           s.IsGraded && s.Grade.HasValue)
                .ToListAsync();

            var distribution = new Dictionary<string, int>
            {
                { "优秀(90-100)", 0 },
                { "良好(80-89)", 0 },
                { "中等(70-79)", 0 },
                { "及格(60-69)", 0 },
                { "不及格(0-59)", 0 }
            };

            foreach (var submission in submissions)
            {
                var grade = submission.Grade!.Value;
                if (grade >= 90) distribution["优秀(90-100)"]++;
                else if (grade >= 80) distribution["良好(80-89)"]++;
                else if (grade >= 70) distribution["中等(70-79)"]++;
                else if (grade >= 60) distribution["及格(60-69)"]++;
                else distribution["不及格(0-59)"]++;
            }

            return distribution;
        }
    }
}

