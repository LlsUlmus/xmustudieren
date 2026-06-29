namespace Ricebird.Framework.TaskService
{
    /// <summary>
    /// 定时运行的任务，每分钟判定一次需要执行的任务
    /// </summary>
    public class TaskService : ITaskService
    {
        protected IServiceProvider Services { get; set; }
        public Dictionary<string, ITask> TaskList = [];
        protected System.Timers.Timer timer = new System.Timers.Timer();
        /// <summary>
        /// 1分钟
        /// </summary>
        public const int ONE_MINIUS = 60000;

        public TaskService(IServiceProvider provider)
        {
            Services = provider;
            Name = "消息服务";
            timer.Interval = ONE_MINIUS;
            timer.Elapsed += delegate
            {
                DoAllTask();
            };
            Start();
        }

        /// <summary>
        /// 添加一个任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(ITask task)
        {
            //添加时就直接执行一次
            using (var scope = Services.CreateScope())
            {
                var client = scope.CreateClient("定时执行服务");
                if (task.RunAtBegin)
                {
                    task.Client = client;
                    DoTask(task, DateTime.Now, scope);
                }
            }
            TaskList.Add(task.Name, task);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="task">该任务是否需要从列表中删除</param>
        /// <returns></returns>
        protected bool DoTask(ITask task, DateTime baseTime, IServiceScope scope)
        {
            //Step1. 断定该任务是否应该执行
            if ((baseTime - task.LastRun).TotalMilliseconds >= task.Interval * ONE_MINIUS)
            {
                //Step2. 判定执行次数是否已经达到要求
                if (task.RunTime > 0 && task.Running >= task.RunTime)
                {
                    return true;
                }

                var client = scope.CreateClient($"定时{DateTime.Now:H:m}执行");
                try
                {
                    task.Client = client;
                    //如果在执行区间里，执行任务
                    task.Doer?.Invoke(scope.ServiceProvider);

                    task.LastRun = baseTime;
                    task.Running++;

                }
                catch (Exception ex)
                {
                    client.LogException(ex, nameof(TaskService), $"{nameof(DoTask)}-{task.Name}");
                }
                return false;
            }

            return false;
        }

        protected void DoAllTask()
        {
            List<string> removeList = [];
            DateTime baseTime = DateTime.Now;
            using IServiceScope scope = Services.CreateScope();
            foreach (var taskToken in TaskList)
            {
                ITask task = taskToken.Value;

                if (DoTask(task, baseTime, scope))
                {
                    removeList.Add(taskToken.Key);
                }
            }

            //Step3. 移除所有已经完成的任务
            foreach (var remove in removeList)
            {
                TaskList.Remove(remove);
            }
        }

        public void Start()
        {
            Enable = true;
        }

        public void Stop()
        {
            Enable = false;
        }

        public bool Enable
        {
            get => timer.Enabled;
            set
            {
                if (value)
                {
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                }
            }
        }

        public string Name
        {
            get;
            set;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
