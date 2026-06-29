using Stop = System.Diagnostics.Stopwatch;

namespace Ricebird.Framework
{
    public class Stopwatch
    {
        public static Stopwatch StartNew()
        {
            var st = new Stopwatch();
            st.Start();
            return st;
        }

        private static long GetTimestamp()
        {
            return Stop.GetTimestamp();
        }

        /// <summary>
        /// 时间戳，单位是 ticks
        /// <para>
        /// 1ms = 10,000ticks = 1,000us = 1,000,000ns
        /// </para>
        /// <para>
        /// https://learn.microsoft.com/zh-cn/dotnet/api/system.datetime.ticks?view=net-7.0
        /// </para>
        /// </summary>
        private long lastTimeStamp = 0;
        private long beginTimeStamp = 0;
        internal Stopwatch()
        {
            Start();
        }

        public void Start()
        {
            beginTimeStamp = Stop.GetTimestamp();
            lastTimeStamp = beginTimeStamp;
        }

        /// <summary>
        /// 单位：毫秒
        /// </summary>
        /// <returns></returns>
        public long ElapsedTime
        {
            get
            {
                long current = GetTimestamp();
                long ellapsed = current - lastTimeStamp;
                lastTimeStamp = current;
                return ellapsed / 10000;
            }
        }

        /// <summary>
        /// 单位：毫秒
        /// </summary>
        /// <returns></returns>
        public long TotalElapsed
        {
            get
            {
                long current = GetTimestamp();
                long ellapsed = current - beginTimeStamp;
                return ellapsed / 10000;
            }
        }

        public void SetCurrent()
        {
            lastTimeStamp = GetTimestamp();
        }

        /// <summary>
        /// 判断距离上一次触发该函数，是否已经超过指定时间
        /// <para>
        /// 如果已经超过，记录当前时间
        /// </para>
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool AssertTimeEllapse(TimeSpan timeSpan)
        {
            long duration = (long)timeSpan.TotalMilliseconds * 10000;
            long current = GetTimestamp();

            if (current - lastTimeStamp > duration)
            {
                lastTimeStamp = current;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
