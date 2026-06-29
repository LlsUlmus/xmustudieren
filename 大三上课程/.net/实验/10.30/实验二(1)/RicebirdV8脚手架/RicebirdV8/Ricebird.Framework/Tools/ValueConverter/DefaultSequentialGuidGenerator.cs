namespace Ricebird.Framework.Tools.ValueConverter
{
    /// <summary>
    /// 时间序Guid生成器，可以生成一个<see cref="Guid"/>数据。
    /// 该数据会按照时间序升序排列。在未补录数据的情况下，可以用作ID。
    /// </summary>
    public interface ISequentialGuidGenerator
    {
        /// <summary>
        /// 生成一个适合于MS SQL的<see cref="Guid"/>，补录的位置全部为随机字节。
        /// </summary>
        /// <returns></returns>
        Guid Next();

        /// <summary>
        /// 生成一个适合于MS SQL的<see cref="Guid"/>，实录的位置为降序排列的数字。该字段用于生成新闻等内容的排序索引字段。保存到数据库再查询时：
        /// <para>
        /// 如果按本字段的升序查询，则：先按<paramref name="firstOrder"/>升序排列，再按时间升序排列，再按<paramref name="secondOrder"/>升序排列，最后按时间戳升序排列。
        /// </para>
        /// <para>
        /// 如果按本字段的降序查询，则：先按<paramref name="firstOrder"/>降序排列，再按时间降序排列，最后按<paramref name="secondOrder"/>降序排列，最后按时间戳降序排列。
        /// </para>
        /// <para>
        /// 特别注意：这个函数生成的GUID虽然不会重复，但不能用于ID字段。
        /// </para>
        /// </summary>
        /// <param name="firstOrder">置顶的排序号，取值范围是int</param>
        /// <param name="time">发布时间，最多至2058年</param>
        /// <param name="secondOrder">普通的排序号，取值范围是int</param>
        /// <returns></returns>
        public Guid Next(int firstOrder, DateTime time, int secondOrder);

        /// <summary>
        /// 将根据<see cref="Next(int, DateTime, int)"/>函数生成的Guid中的order属性进行变幻。在变幻过程中，时间戳保持不变。
        /// </summary>
        /// <param name="infoId">根据<see cref="Next(int, DateTime, int)"/>函数生成的Guid</param>
        /// <param name="firstOrder">置顶的排序号，取值范围是int</param>
        /// <param name="time">，最多至2058年</param>
        /// <param name="secondOrder">普通的排序号，取值范围是int</param>
        /// <returns></returns>
        Guid ReplaceOrderInfo(Guid infoId, int firstOrder, DateTime time, int secondOrder);
    }

    public class DefaultSequentialGuidGenerator : ISequentialGuidGenerator
    {
        private int _counter = 0;
        private static readonly DateTime baseTime = new DateTime(1990, 1, 1);
        private static readonly int[] rgiGuidOrder =
        [
            10, 11, 12, 13, 14, 15, 8, 9, 6, 7,
            4, 5, 0, 1, 2, 3
        ];
        private static readonly DateTime maxValue = baseTime.AddSeconds(int.MaxValue);

        public DefaultSequentialGuidGenerator()
        {
            _counter = (int)(DateTime.Now - baseTime).TotalSeconds;
        }

        /// <summary>
        /// 生成一个适合于MS SQL的<see cref="Guid"/>，补录的位置全部为随机字节。
        /// </summary>
        /// <returns></returns>
        public Guid Next()
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid(guidBytes);
        }

        private static byte[] GetShortBytes(int order, string argName)
        {
            byte[] bytes;
            if (order is <= short.MinValue or > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(argName, $"排序号{argName}的取值范围为[-32767, 32767]");
            }
            else
            {
                int absOrder = 0x8000 + order;
                bytes = BitConverter.GetBytes(absOrder);
            }

            return bytes;
        }

        /// <summary>
        /// 生成一个适合于MS SQL的<see cref="Guid"/>，实录的位置为降序排列的数字。该字段用于生成新闻等内容的排序索引字段。保存到数据库再查询时：
        /// <para>
        /// 如果按本字段的升序查询，则：先按<paramref name="firstOrder"/>升序排列，再按时间升序排列，再按<paramref name="secondOrder"/>升序排列，最后按时间戳升序排列。
        /// </para>
        /// <para>
        /// 如果按本字段的降序查询，则：先按<paramref name="firstOrder"/>降序排列，再按时间降序排列，最后按<paramref name="secondOrder"/>降序排列，最后按时间戳降序排列。
        /// </para>
        /// <para>
        /// 特别注意：这个函数生成的GUID虽然不会重复，但不能用于ID字段。
        /// </para>
        /// </summary>
        /// <param name="firstOrder">置顶的排序号，取值范围是int</param>
        /// <param name="time">发布时间，最多至2058年</param>
        /// <param name="secondOrder">普通的排序号，取值范围是int</param>
        /// <returns></returns>
        public Guid Next(int firstOrder, DateTime time, int secondOrder)
        {
            if (time > maxValue)
            {
                time = maxValue;
            }
            var guidBytes = Guid.NewGuid().ToByteArray();
            var firstOrderBytes = GetShortBytes(firstOrder, nameof(firstOrder));
            int second = (int)(time - baseTime).TotalSeconds;
            var counterBytes = BitConverter.GetBytes(second);
            var secondOrderBytes = GetShortBytes(secondOrder, nameof(secondOrder));
            var timeBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            // 这里转换后的ID全部是小端，也是高位在最后。
            // 所以，下面排序号越大的，对应的位就越小。
            byte[] orderByte =
            [
                firstOrderBytes[3],
                firstOrderBytes[2],
                firstOrderBytes[1],
                firstOrderBytes[0],
                counterBytes[3],
                counterBytes[2],
                counterBytes[1],
                counterBytes[0],
                secondOrderBytes[3],
                secondOrderBytes[2],
                secondOrderBytes[1],
                secondOrderBytes[0],
                timeBytes[3],
                timeBytes[2],
                timeBytes[1],
                timeBytes[0],
            ];
            for (int i = 0; i < 16; i++)
            {
                int order = rgiGuidOrder[i];
                guidBytes[order] = orderByte[i];
            }

            return new Guid(guidBytes);
        }

        /// <summary>
        /// 将根据<see cref="Next(int, DateTime, int)"/>函数生成的Guid中的order属性进行变幻。在变幻过程中，时间戳保持不变。
        /// </summary>
        /// <param name="infoId">根据<see cref="Next(int, DateTime, int)"/>函数生成的Guid</param>
        /// <param name="firstOrder">置顶的排序号，取值范围是int</param>
        /// <param name="time">，最多至2058年</param>
        /// <param name="secondOrder">普通的排序号，取值范围是int</param>
        /// <returns></returns>
        public Guid ReplaceOrderInfo(Guid infoId, int firstOrder, DateTime time, int secondOrder)
        {
            if (time > maxValue)
            {
                time = maxValue;
            }
            var guidBytes = infoId.ToByteArray();
            var firstOrderBytes = GetShortBytes(firstOrder, nameof(firstOrder));
            int second = (int)(time - baseTime).TotalSeconds;
            var counterBytes = BitConverter.GetBytes(second);
            var secondOrderBytes = GetShortBytes(secondOrder, nameof(secondOrder));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }
            // 这里转换后的ID全部是小端，也是高位在最后。
            // 所以，下面排序号越大的，对应的位就越小。
            byte[] orderByte = new byte[16];
            orderByte[0] = firstOrderBytes[3];
            orderByte[1] = firstOrderBytes[2];
            orderByte[2] = firstOrderBytes[1];
            orderByte[3] = firstOrderBytes[0];
            orderByte[4] = counterBytes[3];
            orderByte[5] = counterBytes[2];
            orderByte[6] = counterBytes[1];
            orderByte[7] = counterBytes[0];
            orderByte[8] = secondOrderBytes[3];
            orderByte[9] = secondOrderBytes[2];
            orderByte[10] = secondOrderBytes[1];
            orderByte[11] = secondOrderBytes[0];

            for (int i = 0; i < 12; i++)
            {
                int order = rgiGuidOrder[i];
                guidBytes[order] = orderByte[i];
            }

            return new Guid(guidBytes);
        }
    }
}
