namespace Ricebird.Framework
{
    /// <summary>
    /// 这是一个不对外的日志器，专用写运行日志用。没有依赖关系，并且线程安全
    /// </summary>
    internal class FileLogger : IDisposable
    {
        internal string LogPath
        {
            get; set;
        } = string.Empty;

        private readonly object _lock = new();

        //private FileStream Stream
        //{
        //    get;
        //    set;
        //}

        private Stopwatch Counter
        {
            get; set;
        }

        internal FileLogger(string appPath, Stopwatch counter)
        {
            Counter = counter;
            string dir = Path.Combine(appPath, "Logs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            LogPath = Path.Combine(dir, $"{DateTime.Now:yyyy年M月d日H时m分s秒fff毫秒}的运行日志.txt");
            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }

            // Stream = new FileStream(LogPath, FileMode.Create);
        }

        internal void WriteLog(params string[] logs)
        {
            lock (_lock)
            {
                foreach (var item in logs)
                {
                    Console.WriteLine(item);
                    //var bytes = Encoding.UTF8.GetBytes(item + "\r\n");
                    //Stream.Write(bytes, 0, bytes.Length);
                    //Stream.Flush();
                }
            }
        }



        internal void WriteLog(string module, string log)
        {
            WriteLog($"[{DateTime.Now:H时m分s秒}]{module}(+{Counter.ElapsedTime}ms)：{log}");
        }

        internal void InitialEnd()
        {
            WriteLog($"({Counter.TotalElapsed}ms)系统全部初始化完毕，已可以正常运行。");
        }

        public void Dispose()
        {
            // Stream.Dispose();
        }
    }
}
