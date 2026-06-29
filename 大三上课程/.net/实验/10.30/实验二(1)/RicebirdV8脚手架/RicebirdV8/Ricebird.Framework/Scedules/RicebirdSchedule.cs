using NPOI.SS.UserModel;
using Ricebird.Framework.Clients;
using Ricebird.Framework.FileStorage;
using Ricebird.Framework.Security;
using Ricebird.Framework.Security.Apis;
using Ricebird.Framework.SignalR;

namespace Ricebird.Framework.Scedules
{
    public class RicebirdSchedule(ISmsService sms, IFileStorageService fileService, IClient client, string name, HostEnv hostEnv)
    {
        #region 字段
        public Guid ID
        {
            get; set;
        } = Guid.NewGuid();

        public Guid UserId
        {
            get; set;
        } = client.CurrentUser.ID;

        public string LinkToApi
        {
            get; set;
        } = string.Empty;

        public ApiResult Result
        {
            get; set;
        } = ApiResult.Success;

        public virtual string Type => "普通任务";

        public string Name
        {
            get; set;
        } = name;

        public DateTime CreatedOn
        {
            get; set;
        } = DateTime.Now;

        public IFile? File
        {
            get; set;
        } = null;

        public string DownloadFileName
        {
            get; set;
        } = string.Empty;

        public DateTime ExpiredOn
        {
            get; set;
        } = DateTime.Now;

        public ScheduleStatus Status
        {
            get; set;
        } = ScheduleStatus.Pending;

        public int Current
        {
            get; set;
        } = 0;

        public int Total
        {
            get; set;
        } = 0;

        /// <summary>
        /// 一个百分比，指示任务进度
        /// </summary>
        public double Progress
        {
            get; set;
        } = 0;

        public List<string> Logs
        {
            get; set;
        } = [];

        public CancellationTokenSource CancellationTokenSource
        {
            get; init;
        } = new CancellationTokenSource();
        #endregion

        #region 计算字段
        public string Duration => $"{(DateTime.Now - CreatedOn).Seconds}秒";

        public string ProgressText => $"{Progress:F2}%";

        public string ScheduleDownloadPath => File == null ? "" : $"/api/schedule/download/{ID}";
        #endregion

        public void CreateSchedule()
        {
            sms.SendToUser(UserId, "ricebird-schedule-create", ID, Name, Type);
        }

        /// <summary>
        /// 向前端发送进度日志，这项日志不会显示在前端的界面上，只做为进度条显示。
        /// </summary>
        /// <param name="log"></param>
        /// <param name="current"></param>
        /// <param name="total"></param>
        public void ProgressReport(string log, int current, int total)
        {
            Current = current;
            Total = total;
            Progress = current * 100d / total;
            sms.SendToUser(UserId, "ricebird-schedule-progress", log, ID, current, total, Duration, ProgressText);
        }

        /// <summary>
        /// 向前端发送日志，这项日志会显示在前端的界面上，也会显示进度。
        /// </summary>
        /// <param name="log"></param>
        /// <param name="current"></param>
        /// <param name="total"></param>
        public void LogReport(string log, int current, int total)
        {
            Current = current;
            Total = total;
            Progress = current * 100d / total;
            Logs.Insert(0, log);
            sms.SendToUser(UserId, "ricebird-schedule-log", log, ID, current, total, Duration, ProgressText);
        }

        #region 完成方法
        public void Completed(string log, IWorkbook excelFile, string srcFileName)
        {
            Completed(log, excelFile, srcFileName, TimeSpan.FromMinutes(15));
        }

        public void Completed(string log, IWorkbook excelFile, string srcFileName, TimeSpan expiredOn)
        {
            var bytes = excelFile.ExportBytes();
            Completed(log, bytes, srcFileName, expiredOn);
        }

        public void Completed(string log, Stream stream, string srcFileName)
        {
            Completed(log, stream, srcFileName, TimeSpan.FromMinutes(15));
        }

        public void Completed(string log, byte[] bytes, string srcFileName)
        {
            Completed(log, bytes, srcFileName, TimeSpan.FromMinutes(15));
        }

        public void Completed(string log, Stream stream, string srcFileName, TimeSpan expiredOn)
        {
            var (_, file) = fileService.CreateTemporaryFile(stream, srcFileName);
            DownloadFileName = srcFileName;
            Completed(log, file, expiredOn);
        }

        public void Completed(string log, byte[] bytes, string srcFileName, TimeSpan expiredOn)
        {
            var (_, file) = fileService.CreateTemporaryFile(bytes, srcFileName);
            DownloadFileName = srcFileName;
            Completed(log, file, expiredOn);
        }

        public void Completed(string log)
        {
            Completed(log, null, TimeSpan.FromMinutes(5));
        }

        public void Completed(string log, IFile? download)
        {
            Completed(log, download, TimeSpan.FromMinutes(15));
        }

        public void Completed(string log, TimeSpan expiredOn)
        {
            Completed(log, null, expiredOn);
        }

        public void Completed(string log, IFile? download, TimeSpan expiredOn)
        {
            File = download;
            ExpiredOn = DateTime.Now + expiredOn;
            Progress = 100;
            Status = ScheduleStatus.Completed;
            Current = Total;
            Logs.Insert(0, log);
            Result = ApiResult.Success;
            sms.SendToUser(UserId, "ricebird-schedule-complete", log, ID, ScheduleDownloadPath, Duration, ProgressText);
        }

        public void Cancel(string log)
        {
            Status = ScheduleStatus.Cancel;
            ExpiredOn = DateTime.Now + TimeSpan.FromMinutes(5);
            Current = Total;
            Logs.Insert(0, log);
            Result = ApiResult.Failure;
            sms.SendToUser(UserId, "ricebird-schedule-cancel", log, ID, Duration, ProgressText);
        }
        #endregion

        #region 执行上下文
        protected const string MODULE_NAME = "任务管理模块";
        /// <summary>
        /// 执行任务，不使用async/await
        /// <para>
        /// <b>不可以直接在函数体内使用外部输入的Client，那里面的内容均已释放！</b>
        /// </para>
        /// <para>
        /// <b>绝对不能在函数中保留对参数的引用！另外在上下文中，HttpContext已被释放。</b>
        /// </para>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public RicebirdSchedule ExecuteAsync(Action<RicebirdSchedule, IClient> executor)
        {
            return ExecuteAsync((s, c) =>
            {
                executor(s, c);
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// 执行任务，使用async/await
        /// <para>
        /// <b>不可以直接在函数体内使用外部输入的Client，那里面的内容均已释放！</b>
        /// </para>
        /// <para>
        /// <b>绝对不能在函数中保留对参数的引用！</b>
        /// </para>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public virtual RicebirdSchedule ExecuteAsync(Func<RicebirdSchedule, IClient, Task> executor)
        {
            // 一个任务不能调用两次 ExecuteAsync
            if (Status != ScheduleStatus.Pending)
            {
                return this;
            }

            Stopwatch watch = Stopwatch.StartNew();
            Status = ScheduleStatus.Executing;
            CancellationToken cancellationToken = CancellationTokenSource.Token;
            CreateSchedule();
            IServiceScope scope = HostEnv.ServiceProvider.CreateScope();
            IClient c = client.Clone(scope, ID, MODULE_NAME);
            CancellationTokenSource.Token.Register(() =>
            {
                Cancel("任务已取消");
                try
                {
                    Log(watch.TotalElapsed, c);
                    c.Dispose();
                    scope.Dispose();
                }
                catch
                {

                }
            });
            Task.Run(async () =>
            {
                try
                {
                    await executor(this, c);
                }
                catch (Exception ex)
                {
                    c.LogException(ex, MODULE_NAME, "ExecuteAsync");

                    if (hostEnv.IsDevelopment())
                    {
                        LogReport($"{ex.StackTrace}", 1, 1);
                        LogReport($"{ex.Message}", 1, 1);
                    }
                    Cancel("任务因异常取消，请检查系统日志。");
                    Result = ApiResult.Exception;
                }
                finally
                {
                    Log(watch.TotalElapsed, c);
                    c.Dispose();
                    scope.Dispose();
                }
            }, cancellationToken);
            return this;
        }

        private bool _isLog = false;
        private void Log(long total, IClient client)
        {
            if (_isLog)
            {
                return;
            }

            ApiManager apiManager = client.Resolve<ApiManager>();
            apiManager.Log(LinkToApi, Result, (int)total, client);

            if (HostEnv.Instance.IsDevelopment())
            {
                sms.SendToUser(UserId, "ricebird-schedule-debug", $"任务{Name}结束，总耗时{total}ms");
            }

            _isLog = true;
        }
        #endregion

        public void Dispose()
        {
            if (File is not null)
            {
                fileService.DeleteTemporaryFile(File);
            }
        }
    }
}
