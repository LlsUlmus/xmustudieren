namespace Ricebird.Scedules.ViewModels
{
    public class ScheduleViewModel(RicebirdSchedule schedule)
    {
        public Guid ID
        {
            get; set;
        } = schedule.ID;

        public string Name
        {
            get; set;
        } = schedule.Name;

        public string Type
        {
            get; set;
        } = schedule.Type;

        public string Duration
        {
            get; set;
        } = schedule.Duration;

        public string Progress
        {
            get; set;
        } = schedule.ProgressText;

        public ScheduleStatus Status
        {
            get; set;
        } = schedule.Status;

        public string DownloadPath
        {
            get; set;
        } = schedule.ScheduleDownloadPath;

        public string ExpiredOn
        {
            get; set;
        } = DateTime.Now.DateString(schedule.ExpiredOn);

        public int Current
        {
            get; set;
        } = schedule.Current;

        public int Total
        {
            get; set;
        } = schedule.Total;

        public List<string> Logs
        {
            get; set;
        } = schedule.Logs;
    }
}
