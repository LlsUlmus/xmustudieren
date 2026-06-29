namespace System
{
    public static class GuidExtensions
    {
        /// <summary>
        /// 将Guid转换为62进制的字符串
        /// </summary>
        /// <param name="g"></param>
        /// <returns>长度为22的字符串</returns>
        public static string To62String(this Guid g)
        {
            NumerationSystem ns = new NumerationSystem(g);
            return ns.To62System();
        }
    }
}
