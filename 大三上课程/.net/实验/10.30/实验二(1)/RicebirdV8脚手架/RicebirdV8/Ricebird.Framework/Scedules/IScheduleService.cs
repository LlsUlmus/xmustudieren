using Ricebird.Framework.Clients;

namespace Ricebird.Framework.Scedules
{
    public interface IScheduleService : ISingletonDependency
    {
        RicebirdSchedule CreateSchedule(IClient client, string name);
        RicebirdSchedule CreateSchedule(RicebirdSchedule schedule);
    }
}
