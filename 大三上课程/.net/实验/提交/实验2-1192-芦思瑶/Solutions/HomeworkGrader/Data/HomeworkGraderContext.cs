using Microsoft.EntityFrameworkCore;
using HomeworkGrader.Models;

namespace HomeworkGrader.Data
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class HomeworkGraderContext : DbContext
    {
        public HomeworkGraderContext(DbContextOptions<HomeworkGraderContext> options) : base(options)
        {
        }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<StudentSubmission> StudentSubmissions { get; set; }
        public DbSet<GradingRecord> GradingRecords { get; set; }
        public DbSet<CourseStatistics> CourseStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Assignment实体
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasIndex(e => new { e.CourseName, e.ClassName })
                      .HasDatabaseName("IX_Assignment_Course_Class");
                
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                
                entity.Property(e => e.CourseName)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.ClassName)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // 配置StudentSubmission实体
            modelBuilder.Entity<StudentSubmission>(entity =>
            {
                entity.HasIndex(e => new { e.AssignmentId, e.StudentId })
                      .HasDatabaseName("IX_Submission_Assignment_Student")
                      .IsUnique();
                
                entity.Property(e => e.StudentId)
                      .IsRequired()
                      .HasMaxLength(50);
                
                entity.Property(e => e.StudentName)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.HasOne(d => d.Assignment)
                      .WithMany(p => p.Submissions)
                      .HasForeignKey(d => d.AssignmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置GradingRecord实体
            modelBuilder.Entity<GradingRecord>(entity =>
            {
                entity.HasOne(d => d.Submission)
                      .WithMany()
                      .HasForeignKey(d => d.SubmissionId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.GraderName)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // 配置CourseStatistics实体
            modelBuilder.Entity<CourseStatistics>(entity =>
            {
                entity.HasIndex(e => new { e.CourseName, e.ClassName })
                      .HasDatabaseName("IX_CourseStatistics_Course_Class")
                      .IsUnique();
                
                entity.Property(e => e.CourseName)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.ClassName)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // 种子数据
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 添加示例作业
            modelBuilder.Entity<Assignment>().HasData(
                new Assignment
                {
                    Id = 1,
                    Title = "数据结构与算法作业1",
                    Description = "完成链表、栈、队列的基本操作实现",
                    CourseName = "数据结构与算法",
                    ClassName = "计算机科学与技术2021-1班",
                    CreatedDate = DateTime.Now.AddDays(-7),
                    DueDate = DateTime.Now.AddDays(3),
                    TotalPoints = 100,
                    IsActive = true
                },
                new Assignment
                {
                    Id = 2,
                    Title = "软件工程课程设计",
                    Description = "完成一个完整的软件项目开发",
                    CourseName = "软件工程",
                    ClassName = "软件工程2021-1班",
                    CreatedDate = DateTime.Now.AddDays(-14),
                    DueDate = DateTime.Now.AddDays(7),
                    TotalPoints = 150,
                    IsActive = true
                }
            );
        }
    }
}

