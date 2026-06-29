namespace Ricebird.Framework
{
    internal struct NumerationSystem
    {
        /// <summary>
        /// 进制的基数，不得大于256
        /// </summary>
        private int _base;
        public int Base
        {
            readonly get => _base;
            set
            {
                if (value is <= 0 or > 256)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "进制的基数必须在(0,256]之间");
                }
                _base = value;
            }
        }

        private const string alphabets = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@";

        public NumerationSystem(byte @base)
        {
            Base = @base;
            Digit = [];
        }

        public NumerationSystem(byte[] bytes)
        {
            Base = 256;
            Digit = bytes;
            Array.Reverse(Digit);
        }

        public NumerationSystem(Guid g)
        {
            Base = 256;
            Digit = g.ToByteArray();
            Array.Reverse(Digit);
        }

        public NumerationSystem(int d)
        {
            Base = 10;
            string str = d.ToString();
            Digit = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                Digit[str.Length - 1 - i] = (byte)(str[i] - '0');
            }
        }

        public NumerationSystem(string d62)
        {
            Base = 62;
            Digit = new byte[d62.Length];
            for (int i = 0; i < d62.Length; i++)
            {
                char c = d62[i];
                byte b = c switch
                {
                    >= '0' and <= '9' => (byte)(c - '0'),
                    >= 'A' and <= 'Z' => (byte)(c - 'A' + 10),
                    >= 'a' and <= 'z' => (byte)(c - 'a' + 36),
                    _ => throw new ArgumentOutOfRangeException(nameof(d62), $"字符串必须由0-9，a-z，A-Z的字符组成"),
                };
                Digit[d62.Length - 1 - i] = b;
            }
        }

        public byte[] Digit
        {
            get;
            private set;
        }

        public readonly byte[] ToNumerationSystem(int newBase)
        {
            List<byte> newDigit = [];
            byte[] tmp = new byte[Digit.Length];
            Digit.CopyTo(tmp, 0);
            do
            {
                (tmp, byte remainder) = Divide(tmp, newBase);
                newDigit.Add(remainder);
            } while (tmp.Length != 1 || tmp[0] != 0);

            newDigit.Reverse();
            byte[] data = [.. newDigit];
            return data;
        }

        public readonly string To64System()
        {
            var result = ToNumerationSystem(64);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append(alphabets[b]);
            }

            if (Base == 256)
            {
                return sb.ToString().PadLeft(22, '0');
            }

            return sb.ToString();
        }

        public readonly string To62System(int minLength = 22)
        {
            var result = ToNumerationSystem(62);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append(alphabets[b]);
            }

            if (Base == 256)
            {
                return sb.ToString().PadLeft(minLength, '0');
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取用于顺序Guid的Code字符
        /// </summary>
        /// <returns></returns>
        public readonly string ToCode()
        {
            return To62System(7);
        }

        public readonly int To10System()
        {
            var result = ToNumerationSystem(10);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append("0123456789"[b]);
            }
            return int.Parse(sb.ToString());
        }

        public readonly byte[] To256System(int length)
        {
            var result = ToNumerationSystem(256);
            byte[] bytes = new byte[length];
            if (result.Length <= length)
            {
                Array.Copy(result, 0, bytes, length - result.Length, result.Length);
            }
            else
            {
                throw new InvalidCastException("这个数不合法");
            }

            return bytes;
        }

        public readonly Guid ToGuid()
        {
            var result = ToNumerationSystem(256);
            byte[] bytes = new byte[16];
            if (result.Length <= 16)
            {
                Array.Copy(result, 0, bytes, 16 - result.Length, result.Length);
            }
            else
            {
                throw new InvalidCastException("原数值不是一个合法的GUID");
            }

            Guid g = new Guid(bytes);
            return g;
        }

        public readonly byte[] GetBytes(int length)
        {
            byte[] bytes = new byte[length];

            if (Digit.Length <= length)
            {
                Array.Copy(Digit, 0, bytes, 16 - Digit.Length, Digit.Length);
            }
            else
            {
                throw new InvalidCastException("数值超过界限");
            }

            return bytes;
        }

        internal readonly (byte[] quotient, byte remainder) Divide(byte[] oper1, int oper2)
        {
            List<byte> result = [];
            byte remainder = 0;
            var i = oper1.Length - 1;
            int current = oper1[i];
            while (i >= 0)
            {
                if (current < oper2 && i == 0)
                {
                    // 如果当前值小于除数，并且无法向下借位
                    // 这是余数
                    remainder = (byte)current;
                    break;
                }

                if (current < oper2 && i > 0)
                {
                    // 如果当前值小于除数，但可以向下借位
                    // 向下借一位
                    i--;
                    current = current * Base + oper1[i];
                }

                // 当前值大于除数
                byte quotient = (byte)(current / oper2);
                current %= oper2;

                // 首位的0全部去掉
                if (result.Count != 0 || quotient != 0)
                {
                    result.Add(quotient);
                }
            }

            if (result.Count == 0)
            {
                result.Add(0);
            }

            result.Reverse();

            return (result.ToArray(), remainder);
        }
    }
}
