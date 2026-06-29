using Ricebird.Framework.FileStorage;
using Ricebird.Scedules.ViewModels;
using Timer = System.Timers.Timer;

namespace Ricebird.Scedules.Services
{
    public class ScheduleService : IScheduleService, IDisposable
    {
        public List<RicebirdSchedule> Schedules { get; set; } = [];

        #region 定时清理
        private readonly ISmsService sms;
        private Timer ClearTimer { get; set; }
        private void ClearExpired(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            List<RicebirdSchedule> needRemove = [];
            bool[] needClear = [false, false, true, true];
            foreach (var item in Schedules)
            {
                if (needClear[(int)item.Status] && now > item.ExpiredOn)
                {
                    needRemove.Add(item);
                }

                if ((now - item.CreatedOn).TotalMinutes > 20)
                {
                    item.CancellationTokenSource.Cancel();
                    needRemove.Add(item);
                }
            }

            foreach (var item in needRemove)
            {
                Schedules.Remove(item);
                sms.SendToUser(item.UserId, "ricebird-schedule-clear", item.ID);
                item.Dispose();
            }
        }

        public ScheduleService(ISmsService sms)
        {
            ClearTimer = new Timer(TimeSpan.FromSeconds(1));
            ClearTimer.Elapsed += ClearExpired;
            ClearTimer.Start();
            this.sms = sms;
        }
        #endregion

        public RicebirdSchedule CreateSchedule(IClient client, string name)
        {
            ISmsService sms = client.Resolve<ISmsService>();
            IFileStorageService fileService = client.Resolve<IFileStorageService>();
            RicebirdSchedule schedule = new RicebirdSchedule(sms, fileService, client, name, HostEnv.Instance);
            Schedules.Add(schedule);
            return schedule;
        }

        public RicebirdSchedule CreateSchedule(RicebirdSchedule schedule)
        {
            Schedules.Add(schedule);
            return schedule;
        }

        public (bool success, string msg, List<ScheduleViewModel> data) GetSchedules(Guid userId)
        {
            var query = from s in Schedules
                        where s.UserId == userId
                        orderby s.CreatedOn descending
                        select new ScheduleViewModel(s);

            return (true, "", query.ToList());
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ClearTimer.Stop();
            ClearTimer.Dispose();

            foreach (var item in Schedules)
            {
                item.CancellationTokenSource.Cancel();
            }

            Schedules = [];
        }
    }
}
