namespace Ricebird.Framework.Scedules
{
    [DataDictionary("任务状态")]
    public enum ScheduleStatus : int
    {
        [DataEntry("创建中")]
        Pending,
        [DataEntry("执行中")]
        Executing,
        [DataEntry("已完成")]
        Completed,
        [DataEntry("已取消")]
        Cancel
    }
}
