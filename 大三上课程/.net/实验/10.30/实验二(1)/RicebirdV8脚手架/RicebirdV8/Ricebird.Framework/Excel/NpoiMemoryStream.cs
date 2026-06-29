namespace Ricebird.Framework.Excel
{
    internal class NpoiMemoryStream(bool close = false) : MemoryStream
    {
        /// <summary>
        /// 获取流是否关闭
        /// </summary>
        public bool IsClose
        {
            get;
            set;
        } = close;

        public override void Close()
        {
            if (IsClose)
            {
                base.Close();
            }
        }
    }
}
