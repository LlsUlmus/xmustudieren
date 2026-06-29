using Ricebird.Framework.Clients;

namespace Ricebird.Framework.TaskService
{
    public class Task : ITask
    {
        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="interval">执行间隔时间</param>
        /// <param name="doer">执行内容</param>
        /// <param name="runTime"></param>
        public Task(string name, int interval, int runTime = 0)
        {
            Name = name;
            Interval = interval;
            RunTime = runTime;
            Running = 0;
        }

        public IClient Client
        {
            get; set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public int Interval
        {
            get;
            set;
        }

        public DateTime LastRun
        {
            get;
            set;
        }

        public Action<IServiceProvider> Doer
        {
            get;
            set;
        }

        public int RunTime
        {
            get;
            set;
        }

        public int Running
        {
            get;
            set;
        }

        public bool RunAtBegin
        {
            get; set;
        } = true;
    }
}
