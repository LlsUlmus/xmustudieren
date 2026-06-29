using System.Collections;

namespace Ricebird.Framework
{
    public readonly struct SystemCode
    {
        private readonly byte[] data = new byte[5];
        private readonly string code;
        public SystemCode()
        {
            Random.Shared.NextBytes(data);
            code = new NumerationSystem((byte[])data.Clone()).To62System(7);
        }

        public SystemCode(IEnumerable<byte> bytes)
        {
            data = bytes.ToArray();
            code = new NumerationSystem((byte[])data.Clone()).To62System(7);
        }

        public SystemCode(string code)
        {
            data = new NumerationSystem(code).To256System(5);
            this.code = code;
        }

        public override string ToString() => code;

        public static implicit operator string(SystemCode systemCode)
        {
            return systemCode.code;
        }

        public static implicit operator byte[](SystemCode systemCode)
        {
            return systemCode.data;
        }

        public static implicit operator Guid(SystemCode systemCode)
        {
            byte[] bytes = new byte[16];
            Array.Copy(systemCode.data, bytes, 4);
            return new Guid(bytes);
        }
    }

    /// <summary>
    /// 在数据库中，保持升序的Guid序列
    /// </summary>
    public struct SequentialGuid : IStructuralEquatable
    {
        #region 静态内容
        private static long _counter = GetInternalTimestamp(DateTime.Now);
        private static long GetInternalTimestamp(DateTime now) => (long)(now - baseTime).TotalMilliseconds;
        private static readonly DateTime baseTime = new DateTime(2023, 1, 1);
        private static readonly int[] rgiGuidOrder =
        [
            10, 11, 12, 13, 14, 15, 08, 09, 06, 07, 04, 05, 00, 01, 02, 03
        ];
        private static readonly int[] netGuidOrder = [
            12, 13, 14, 15, 10, 11, 08, 09, 06, 07, 00, 01, 02, 03, 04, 05
        ];
        public static long Counter => Interlocked.Increment(ref _counter);

        private readonly byte[] data = new byte[16];

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal static byte[] SystemId
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            get; set;
        }
        #endregion

        #region ctor
        public SequentialGuid()
        {
            byte[] creator = new byte[5];
            Random.Shared.NextBytes(creator);
            NextId(creator, SystemId);
        }

        public SequentialGuid(string creator) : this()
        {
            NumerationSystem str = new NumerationSystem(creator);
            var bytes = str.GetBytes(5);
            NextId(bytes, SystemId);
        }

        public SequentialGuid(byte[] creator) : this()
        {
            NextId(creator, SystemId);
        }

        public SequentialGuid(byte[] creator, byte[] sId)
        {
            NextId(creator, sId);
        }

        /// <summary>
        /// 从数据库的Guid反向转入
        /// 这时需要将序号转回正常的序号
        /// </summary>
        /// <param name="guid"></param>
        public SequentialGuid(Guid guid)
        {
            var bytes = guid.ToByteArray();

            // 以下代码可以将Sql顺序，转回正常顺序
            for (int i = 0; i < 16; i++)
            {
                int order = netGuidOrder[i];
                data[order] = bytes[i];
            }
        }
        #endregion

        #region 转换方法
        public readonly string GetCreator()
        {
            byte[] bytes = new byte[5];
            Array.Copy(data, 11, bytes, 0, 5);
            return new SystemCode(bytes.Reverse());
        }

        public readonly string GetSystemId()
        {
            byte[] bytes = new byte[5];
            Array.Copy(data, 0, bytes, 0, 5);
            return new SystemCode(bytes.Reverse());
        }

        private readonly SequentialGuid NextId(byte[] creator, byte[] systemId)
        {
            byte[] timeStamp = BitConverter.GetBytes(Counter);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeStamp);
            }

            byte[] abcd = [0];
            data[00] = systemId[4];
            data[01] = systemId[3];
            data[02] = systemId[2];
            data[03] = systemId[1];
            data[04] = systemId[0];
            data[05] = timeStamp[7];
            data[06] = timeStamp[6];
            data[07] = timeStamp[5];
            data[08] = timeStamp[4];
            data[09] = timeStamp[3];
            data[10] = timeStamp[2];
            data[11] = timeStamp[1];
            data[12] = timeStamp[0];
            data[13] = creator[2];
            data[14] = creator[1];
            data[15] = creator[0];

            return this;
        }

        private readonly Guid ToSqlGuid()
        {
            byte[] bytes = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                int order = rgiGuidOrder[i];
                bytes[order] = data[i];
            }

            return new Guid(bytes);
        }

        public readonly string To62String()
        {
            return ToSqlGuid().To62String();
        }

        public override readonly string ToString()
        {
            return ToString("D");
        }

        public readonly string ToString([StringSyntax(StringSyntaxAttribute.GuidFormat)] string? format)
        {
            return ToSqlGuid().ToString(format);
        }

        public static implicit operator SequentialGuid(Guid guid) => new SequentialGuid(guid);

        public static implicit operator Guid(SequentialGuid suid) => suid.ToSqlGuid();
        #endregion

        #region 创建方法
        public static Guid NewGuid() => Guid.NewGuid();
        public static SequentialGuid NewSuid() => new SequentialGuid();
        public static SequentialGuid NewSuid(string creator) => new SequentialGuid(creator);
        public static SequentialGuid NewSuid(byte[] creator) => new SequentialGuid(creator);
        public static SequentialGuid NewSuid(string creator, string systemId)
        {
            NumerationSystem str = new NumerationSystem(creator);
            var creatorByte = str.GetBytes(5);
            str = new NumerationSystem(systemId);
            var systemBytes = str.GetBytes(5);

            return new SequentialGuid(creatorByte, systemBytes);
        }

        public static SequentialGuid From62String(string suid)
        {
            _ = suid.TryParseToGuid(out Guid g);

            return g;
        }
        #endregion

        #region 相等方法
        public static bool operator ==(SequentialGuid x, SequentialGuid y) => x.Equals(y);
        public static bool operator ==(SequentialGuid x, Guid y) => x.Equals(new SequentialGuid(y));
        public static bool operator ==(Guid x, SequentialGuid y) => y.Equals(new SequentialGuid(x));
        public static bool operator !=(SequentialGuid x, SequentialGuid y) => !(x == y);
        public static bool operator !=(SequentialGuid x, Guid y) => !(x == y);
        public static bool operator !=(Guid x, SequentialGuid y) => !(x == y);

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null) return false;

            if (obj is SequentialGuid y)
            {
                return data.SequenceEqual(y.data);
            }

            return false;
        }

        public override readonly int GetHashCode()
        {
            return data.GetHashCode();
        }

        public readonly bool Equals(object? other, IEqualityComparer comparer)
        {
            return Equals(other);
        }

        public readonly int GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode();
        }
        #endregion
    }
}
