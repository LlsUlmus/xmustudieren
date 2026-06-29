namespace Ricebird.Framework.TaskService
{
    public class TimedTask : Task
    {
        public TimedTask(string name, int interval)
            : this(name, DateTime.Now.AddDays(-1), interval, 0)
        {

        }

        public TimedTask(string name, DateTime startTime, int interval, int runTime = 0)
            : base(name, interval, runTime)
        {
            LastRun = startTime;
        }
    }
}