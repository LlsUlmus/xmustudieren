
using Ricebird.Framework.Clients;

namespace Ricebird.Framework.TaskService
{
    /// <summary>
    /// 任务
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 任务名
        /// </summary>
        string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 执行间隔 分钟
        /// </summary>
        int Interval
        {
            get;
            set;
        }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        DateTime LastRun
        {
            get;
            set;
        }

        IClient Client
        {
            get; set;
        }

        /// <summary>
        /// 执行内容
        /// </summary>
        Action<IServiceProvider> Doer
        {
            get;
            set;
        }

        /// <summary>
        /// 执行总次数，小于等于0则为永远运行
        /// </summary>
        int RunTime
        {
            get;
            set;
        }

        /// <summary>
        /// 已经执行的次数
        /// </summary>
        int Running
        {
            get;
            set;
        }

        bool RunAtBegin
        {
            get;
        }
    }
}
