namespace Ricebird.Diagnostics.Services
{
    internal class DefaultMemoryLogger : IMemoryLogger
    {
        private readonly List<string> logs = [];

        public List<string> Logs => logs;

        public DefaultMemoryLogger()
        {

        }
        public void Add(string msg) => logs.Add(msg);
    }
}
