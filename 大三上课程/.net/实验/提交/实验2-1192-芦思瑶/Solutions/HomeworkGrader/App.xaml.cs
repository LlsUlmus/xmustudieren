using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HomeworkGrader.Data;
using HomeworkGrader.Services;
using HomeworkGrader.ViewModels;

namespace HomeworkGrader
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider? Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 配置服务
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            // 确保数据库创建
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HomeworkGraderContext>();
            context.Database.EnsureCreated();

            // 显示主窗口
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // 数据库
            services.AddDbContext<HomeworkGraderContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // 服务
            services.AddTransient<AssignmentService>();
            services.AddTransient<SubmissionService>();
            services.AddTransient<StatisticsService>();
            services.AddTransient<FileService>();
            services.AddTransient<ExportService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<AssignmentViewModel>();
            services.AddTransient<GradingViewModel>();
            services.AddTransient<StatisticsViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<AssignmentWindow>();
            services.AddTransient<GradingWindow>();
            services.AddTransient<StatisticsWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Services?.Dispose();
            base.OnExit(e);
        }
    }
}
