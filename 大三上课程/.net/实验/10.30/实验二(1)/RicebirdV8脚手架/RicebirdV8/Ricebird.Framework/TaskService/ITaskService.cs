namespace Ricebird.Framework.TaskService
{
    /// <summary>
    /// 计划任务
    /// </summary>
    public interface ITaskService : ISingletonDependency
    {
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        void AddTask(ITask task);
    }
}
