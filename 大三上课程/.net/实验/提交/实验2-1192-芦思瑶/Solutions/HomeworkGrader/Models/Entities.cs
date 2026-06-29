using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeworkGrader.Models
{
    /// <summary>
    /// 作业实体
    /// </summary>
    public class Assignment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string ClassName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime DueDate { get; set; }
        
        public int TotalPoints { get; set; } = 100;
        
        public bool IsActive { get; set; } = true;
        
        // 导航属性
        public virtual ICollection<StudentSubmission> Submissions { get; set; } = new List<StudentSubmission>();
    }

    /// <summary>
    /// 学生提交的作业
    /// </summary>
    public class StudentSubmission
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AssignmentId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string StudentId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string StudentName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string FilePath { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;
        
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        
        public bool IsLate { get; set; } = false;
        
        public int? Grade { get; set; }
        
        [MaxLength(1000)]
        public string Feedback { get; set; } = string.Empty;
        
        public DateTime? GradedDate { get; set; }
        
        public bool IsGraded { get; set; } = false;
        
        // 导航属性
        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;
    }

    /// <summary>
    /// 批改记录
    /// </summary>
    public class GradingRecord
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SubmissionId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string GraderName { get; set; } = string.Empty;
        
        public DateTime GradedDate { get; set; } = DateTime.Now;
        
        public int Grade { get; set; }
        
        [MaxLength(1000)]
        public string Comments { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string DetailedFeedback { get; set; } = string.Empty;
        
        // 导航属性
        [ForeignKey("SubmissionId")]
        public virtual StudentSubmission Submission { get; set; } = null!;
    }

    /// <summary>
    /// 课程统计
    /// </summary>
    public class CourseStatistics
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string ClassName { get; set; } = string.Empty;
        
        public int TotalAssignments { get; set; }
        
        public int TotalSubmissions { get; set; }
        
        public int GradedSubmissions { get; set; }
        
        public double AverageGrade { get; set; }
        
        public double HighestGrade { get; set; }
        
        public double LowestGrade { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}

