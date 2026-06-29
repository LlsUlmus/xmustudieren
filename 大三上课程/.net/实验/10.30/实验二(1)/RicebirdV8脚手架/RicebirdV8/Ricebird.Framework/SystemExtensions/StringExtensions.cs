namespace System
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 判断字符串非空
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 判断字符串空或者全为空格
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// IsNullOrWhiteSpace的反函数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 字符串非空，且不包含特定字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str, params string[] ignores)
        {
            return !string.IsNullOrWhiteSpace(str) && !ignores.Contains(str);
        }

        /// <summary>
        /// 从左侧截取字符串
        /// </summary>
        public static string Left(this string str, int len)
        {
            if (str.Length < len)
            {
                return str;
            }

            return str[..len];
        }

        /// <summary>
        /// 使用 <see cref="Environment.NewLine"/> 格式化行尾标识符
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// 替换第一次出现的某字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string str, string search, string replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var pos = str.IndexOf(search, comparisonType);
            if (pos < 0)
            {
                return str;
            }

            return string.Concat(str.AsSpan(0, pos), replace, str.AsSpan(pos + search.Length));
        }

        /// <summary>
        /// 从尾向前截取字符串
        /// </summary>
        public static string Right(this string str, int len)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            if (str.Length < len)
            {
                throw new ArgumentException("截取长度必须小于字符串长度!");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// 格式化字条串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static string FormatString(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// 将字符串反解为列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static IEnumerable<T> ChangeToList<T>(this string str, params char[] separator)
        {
            var ary = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            List<T> result = [];
            foreach (var item in ary)
            {
                T? obj = ValueUtils.ChangeToType<T>(item);
                if (obj == null) continue;
                result.Add(obj);
            }

            return result;
        }

        /// <summary>
        /// 按指定元素分离字符串
        /// </summary>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// 按指定元素分离字符串
        /// </summary>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new[] { separator }, options);
        }

        /// <summary>
        /// 按行分离字符串.
        /// </summary>
        public static string[] SplitToLines(this string str)
        {
            return str.Split(Environment.NewLine);
        }

        /// <summary>
        /// 按行分离字符串.
        /// </summary>
        public static string[] SplitToLines(this string str, StringSplitOptions options)
        {
            return str.Split(Environment.NewLine, options);
        }

        /// <summary>
        /// 分割字符串并进行类型转换，分割结果会去掉空项。所有字符串都会去除头尾空格
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceStr"></param>
        /// <param name="seperator"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T[] SplitTo<T>(this string sourceStr, char seperator, T defaultValue)
        {
            return sourceStr.SplitTo([seperator], defaultValue);
        }

        /// <summary>
        /// 分割字符串并进行类型转换，分割结果会去掉空项。所有字符串都会去除头尾空格
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceStr"></param>
        /// <param name="seperator"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>

        public static T[] SplitTo<T>(this string sourceStr, char[] seperator, T defaultValue)
        {
            return sourceStr.Split(seperator, StringSplitOptions.RemoveEmptyEntries).Select(e =>
            {
                return ValueUtils.ConvertTo(e.Trim(), defaultValue);
            }).ToArray();
        }

        /// <summary>
        /// 从起始位置开始截取指定长度字符串.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string Truncate(this string str, int maxLength)
        {
            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// 从后向前截取指定长度的字符串.
        /// </summary>
        public static string TruncateFromBeginning(this string str, int maxLength)
        {
            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Right(maxLength);
        }

        /// <summary>
        /// 从起始位置截取指定长度的字符串，并且如果字符串总长度大于指定长度，则在默认增加"...".
        /// 最后返回的字符串长度不会超过指定长度
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> 为空</exception>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// 从起始位置截取指定长度的字符串，并且如果字符串总长度大于指定长度，则在默认增加<paramref name="postfix"/>.
        /// 最后返回的字符串长度不会超过指定长度
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> 为空</exception>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            EnsureNotNull(str);

            if (str == string.Empty || maxLength == 0)
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }

        /// <summary>
        /// 将字符串以 <see cref="Encoding.UTF8"/> 的形式转换为字节数组
        /// </summary>
        public static byte[] GetBytes(this string str)
        {
            return str.GetBytes(Encoding.UTF8);
        }

        /// <summary>
        /// 将字符串以 <paramref name="encoding"/> 的形式转换为字节数组
        /// </summary>
        public static byte[] GetBytes(this string str, [NotNull] Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        public static JsonNode ToJsonNode(this string str)
        {
            JsonNode node = JsonNode.Parse(str) ?? EmptyJsonNode();
            return node;
        }

        public static T GetJsonValue<T>(this string str, string path, T defaultValue)
        {
            return ToJsonNode(str).GetValue(path, defaultValue);
        }

        /// <summary>
        /// 将一个长度为22的62进制字符串转换为Guid
        /// </summary>
        /// <param name="str">长度为22的62进制</param>
        /// <param name="g">输出的Guid</param>
        /// <returns>是否成功转换</returns>
        public static bool TryParseToGuid(this string str, out Guid g)
        {
            try
            {
                if (Guid.TryParse(str, out g)) return true;
                NumerationSystem ns = new NumerationSystem(str);
                g = ns.ToGuid();
                return true;
            }
            catch
            {
                g = Guid.Empty;
                return false;
            }
        }

        public static string ShowValue(this string str, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(str) ? defaultValue : str;
        }
    }
}