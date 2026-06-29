using Ricebird.Framework.Tools.ValueConverter;
using SkiaSharp;
using System.Text.RegularExpressions;
using System.Web;

namespace Ricebird.Framework
{
    public static class Utils
    {
        private static readonly ISequentialGuidGenerator _guidGenerator = new DefaultSequentialGuidGenerator();
        public static ISequentialGuidGenerator GuidGenerator => _guidGenerator;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal static HtmlChecker.HtmlChecker htmlChecker;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        #region 密码生成器
        /// <summary>
        /// 生成一串密钥
        /// </summary>
        /// <param name="sourceArray">密文空间</param>
        /// <param name="keyLength">密钥长度</param>
        /// <returns></returns>
        public static string GenerateKey(this string[] sourceArray, int keyLength)
        {
            StringBuilder result = new();
            int aryLength = sourceArray.Length;
            for (int i = 0; i < keyLength; i++)
            {
                result.Append(sourceArray[Random.Shared.Next(aryLength)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 生成一串密钥
        /// </summary>
        /// <param name="sourceArray">密文空间</param>
        /// <param name="keyLength">密钥长度</param>
        /// <returns></returns>
        public static string GenerateKey(this string sourceArray, int keyLength)
        {
            StringBuilder result = new();
            int aryLength = sourceArray.Length;
            for (int i = 0; i < keyLength; i++)
            {
                result.Append(sourceArray[Random.Shared.Next(aryLength)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 生成一个随机数，调用Random.Next。取值范围[minValue, maxValue)
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int minValue, int maxValue)
        {
            return Random.Shared.Next(minValue, maxValue);
        }

        /// <summary>
        /// 生成一个密码
        /// </summary>
        /// <param name="keyLength">密钥长度，密钥空间[a-zA-Z0-9!@#$%^]</param>
        /// <returns></returns>
        public static string GenerateKey(int keyLength)
        {
            return "abcdefghijklmnopqrstuvwxyz!@#$%^ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".GenerateKey(keyLength);
        }

        /// <summary>
        /// 生成一个密码
        /// </summary>
        /// <param name="keyLength">密钥长度，密钥空间[a-zA-Z0-9]</param>
        /// <returns></returns>
        public static string GenerateId(int keyLength)
        {
            return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".GenerateKey(keyLength);
        }
        #endregion

        #region 日期处理
        /// <summary>
        /// 将微博时间转换为DateTime
        /// </summary>
        /// <param name="dateString">微博时间字符串</param>
        /// <returns>DateTime</returns>
        public static DateTime ParseUTCDate(this string dateString)
        {
            try
            {
                System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

                DateTime dt = DateTime.ParseExact(dateString, "ddd MMM dd HH:mm:ss zzz yyyy", provider);

                return dt;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 时间比较，输出几天前，几天后
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static string DateString(this DateTime dt)
        {
            return dt.DateString(DateTime.Now);
        }

        /// <summary>
        /// 时间比较，输出几天前，几天后之类的
        /// </summary>
        /// <param name="dt">当前时间</param>
        /// <param name="compare">被比较的时间</param>
        /// <returns>如果被比较的时间在当前时间之前，显示 多少时间前。反之显示多少时间后</returns>
        public static string DateString(this DateTime dt, DateTime compare)
        {
            TimeSpan span;
            string txt;
            if (dt > compare)
            {
                txt = "前";
                span = dt - compare;
            }
            else
            {
                txt = "后";
                span = compare - dt;
            }

            if (span.TotalDays > 60)
            {
                return dt.ToShortDateString();
            }
            else
            {
                if (span.TotalDays > 30)
                {
                    return
                    "1个月" + txt;
                }
                else
                {
                    if (span.TotalDays > 14)
                    {
                        return
                        "2周" + txt;
                    }
                    else
                    {
                        if (span.TotalDays > 7)
                        {
                            return
                            "1周" + txt;
                        }
                        else
                        {
                            if (span.TotalDays > 1)
                            {
                                return
                                string.Format("{0}天{1}", (int)Math.Floor(span.TotalDays), txt);
                            }
                            else
                            {
                                if (span.TotalHours > 1)
                                {
                                    return
                                    string.Format("{0}小时{1}", (int)Math.Floor(span.TotalHours), txt);
                                }
                                else
                                {
                                    if (span.TotalMinutes > 1)
                                    {
                                        return
                                        string.Format("{0}分钟{1}", (int)Math.Floor(span.TotalMinutes), txt);
                                    }
                                    else
                                    {
                                        if (span.TotalSeconds >= 1)
                                        {
                                            return
                                            string.Format("{0}秒{1}", (int)Math.Floor(span.TotalSeconds), txt);
                                        }
                                        else
                                        {
                                            return
                                            "1秒" + txt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取中文的周几
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetWeekChinese(this DateTime dt)
        {
            string[] days = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];
            string week = days[Convert.ToInt32(dt.DayOfWeek.ToString("d"))].ToString();
            return week;
        }

        /// <summary>  
        /// 得到本周第一天(以星期一为第一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekFirstDayMon(this DateTime datetime)
        {
            //星期一为第一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);

            //因为是以星期一为第一天，所以要判断weeknow等于0时，要向前推6天。  
            weeknow = (weeknow == 0 ? (7 - 1) : (weeknow - 1));
            int daydiff = (-1) * weeknow;

            //本周第一天  
            string FirstDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(FirstDay);
        }
        /// <summary>  
        /// 得到本周最后一天(以星期天为最后一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekLastDaySun(this DateTime datetime)
        {
            //星期天为最后一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);
            weeknow = (weeknow == 0 ? 7 : weeknow);
            int daydiff = (7 - weeknow);

            //本周最后一天  
            string LastDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(LastDay).AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// 转换为Unix时间戳，可以用在JS里
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToUnixMillis(this DateTime date)
        {
            DateTime origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalMilliseconds;
        }

        /// <summary>
        /// 转换为Unix时间戳，可以用在JS里
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToUnixSecond(this DateTime date)
        {
            DateTime origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Max((long)diff.TotalSeconds, 0);
        }

        /// <summary>
        /// 将Unix时间戳转换为C#时间
        /// </summary>
        /// <param name="milliTime"></param>
        /// <returns></returns>
        public static DateTime FromUnixMillis(long milliTime)
        {
            long timeTricks = new DateTime(1970, 1, 1).Ticks + milliTime * 10000 + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours * 36000000000L;
            return new DateTime(timeTricks);
        }

        /// <summary>
        /// 将Unix时间戳转换为C#时间
        /// </summary>
        /// <param name="milliTime"></param>
        /// <returns></returns>
        public static bool TryPaseFromUnixMillis(string str, out DateTime date)
        {
            if (!long.TryParse(str, out long milliTime))
            {
                date = DateTime.Now;
                return false;
            }

            long timeTricks = new DateTime(1970, 1, 1).Ticks + milliTime * 10000 + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours * 36000000000L;
            date = new DateTime(timeTricks);
            return true;
        }
        #endregion

        #region 复制对象
        /// <summary>
        /// Copies the readable and writable public property values from the source object to the target
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="target">The target object</param>
        /// <param name="source">The source object</param>
        public static void CopyPropertiesFrom(this object target, object source)
        {
            CopyPropertiesFrom(target, source, string.Empty);
        }

        /// <summary>
        /// Copies the readable and writable public property values from the source object to the target
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="target">The target object</param>
        /// <param name="source">The source object</param>
        /// <param name="ignoreProperty">A single property name to ignore</param>
        public static void CopyPropertiesFrom(this object target, object source, string ignoreProperty)
        {
            CopyPropertiesFrom(target, source, new[] { ignoreProperty });
        }

        /// <summary>
        /// Copies the readable and writable public property values from the source object to the target
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="target">The target object</param>
        /// <param name="source">The source object</param>
        /// <param name="ignoreProperties">An array of property names to ignore</param>
        public static void CopyPropertiesFrom(this object target, object source, params string[] ignoreProperties)
        {
            // Get and check the object types
            Type type = source.GetType();
            //if (target.GetType() != type)
            //{
            //    throw new ArgumentException("The source type must be the same as the target");
            //}
            Type targetType = target.GetType();

            // Build a clean list of property names to ignore
            var ignoreList = new List<string>();
            foreach (string item in ignoreProperties)
            {
                if (!string.IsNullOrEmpty(item) && !ignoreList.Contains(item))
                {
                    ignoreList.Add(item);
                }
            }

            // Copy the properties
            foreach (PropertyInfo property in type.GetProperties())
            {
                try
                {
                    var targetProp = targetType.GetProperty(property.Name);
                    if (targetProp != null
                        && targetProp.CanWrite
                        && property.CanRead
                        && targetProp.PropertyType == property.PropertyType
                        && !ignoreList.Contains(property.Name))
                    {
                        object? val = null;
                        try
                        {
                            val = property.GetValue(source, null);
                        }
                        catch
                        {
                            continue;
                        }

                        //处理一下，使他不会复制NULL值
                        if (val != null)
                        {
                            targetProp.SetValue(target, val, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
        }

        /// <summary>
        /// Copies the readable and writable public property values from the source object to the target
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="target">The target object</param>
        /// <param name="source">The source object</param>
        /// <param name="prop">An array of property name to copy</param>
        /// <param name="ignoreProperties">An array of property names to ignore</param>
        public static void CopyPropertiesFrom(this object target, object source, IEnumerable<string> prop, params string[] ignoreProperties)
        {
            // Get and check the object types
            Type type = source.GetType();
            //if (target.GetType() != type)
            //{
            //    throw new ArgumentException("The source type must be the same as the target");
            //}
            Type targetType = target.GetType();

            // Build a clean list of property names to ignore
            var ignoreList = new List<string>();
            foreach (string item in ignoreProperties)
            {
                if (!string.IsNullOrEmpty(item) && !ignoreList.Contains(item))
                {
                    ignoreList.Add(item);
                }
            }

            // Copy the properties
            foreach (PropertyInfo property in type.GetProperties())
            {
                var targetProp = targetType.GetProperty(property.Name);
                try
                {
                    if (targetProp != null
                        && targetProp.CanWrite
                        && property.CanRead
                        && targetProp.PropertyType == property.PropertyType
                        && prop.Contains(property.Name)
                        && !ignoreList.Contains(property.Name))
                    {
                        object? val = null;
                        try
                        {
                            val = property.GetValue(source, null);
                        }
                        catch
                        {
                            continue;
                        }

                        //处理一下，使他不会复制NULL值
                        if (val != null)
                        {
                            targetProp.SetValue(target, val, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
        }
        #endregion

        #region Json
        /// <summary>
        /// 序列化Json对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SearializeJson(this object obj, Action<JsonSerializerOptions> optionBuilder)
        {
            return SearializeJson(obj, false, optionBuilder);
        }

        /// <summary>
        /// 序列化Json对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SearializeJson(this object obj, bool hasIndent = false, Action<JsonSerializerOptions>? optionBuilder = null)
        {
            JsonSerializerOptions opt = RicebirdSerializerOption.Default;
            opt.WriteIndented = hasIndent;
            optionBuilder?.Invoke(opt);

            return JsonSerializer.Serialize(obj, options: opt);
        }


        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T? DesearializeJson<T>(string json, Action<JsonSerializerOptions>? optionBuilder = null)
        {
            try
            {
                JsonSerializerOptions opt = RicebirdSerializerOption.Default;

                optionBuilder?.Invoke(opt);

                T? obj1 = JsonSerializer.Deserialize<T>(json, opt);
                return obj1;
            }
            catch
            {
                return default;
            }
        }

        private static readonly JsonNode _emptyNode = JsonNode.Parse("{}")!;
        public static JsonNode EmptyJsonNode()
        {
            return _emptyNode;
        }
        #endregion

        #region 文件相关
        /// <summary>
        /// 子目录联合使用
        /// </summary>
        /// <param name="subPaths"></param>
        /// <returns></returns>
        public static string GetPath(params string[] subPaths)
        {
            string path = Path.GetFullPath(Path.Combine(subPaths));

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// 确保路径存在，如果不存在则新建
        /// </summary>
        /// <param name="path"></param>
        public static void EnsureDirectoryExists(string path)
        {
            string dir = Path.GetDirectoryName(path) ?? string.Empty;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// 打开或者创建文件。如果文件存在，则打开，如果文件不存在则新建。
        /// <para>
        /// 会自动验证目录是否存在
        /// </para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileStream OpenOrCreateFile(string path)
        {
            EnsureDirectoryExists(path);
            if (File.Exists(path))
            {
                return new FileStream(path, FileMode.Open);
            }
            else
            {
                return new FileStream(path, FileMode.Create);
            }
        }

        public static string GetSafeFileName(string fileName)
        {
            StringBuilder rBuilder = new StringBuilder(fileName);
            foreach (char rInvalidChar in Path.GetInvalidFileNameChars())
            {
                rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
            }
            return rBuilder.ToString().Trim();
        }
        #endregion

        #region 字符串相关
        public static bool AnyStringHasContent(params string[] str)
        {
            bool flag = false;

            foreach (string s in str)
            {
                if (!string.IsNullOrEmpty(s)) return true;
            }

            return flag;
        }

        public static bool AllStringsHasContent(params string[] str)
        {
            foreach (string s in str)
            {
                if (string.IsNullOrEmpty(s)) return false;
            }

            return true;
        }

        public static string ToUtf8String(this byte[] bytes)
        {
            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region 罗马数字相关
        /// <summary>
        /// 将数字转换为罗马数字
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string IntToRoman(int num)
        {
            int[] values = [1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1];
            StringBuilder sb = new StringBuilder();// Loop through each symbol, stopping if num becomes 0.
            for (int i = 0; i < values.Length && num >= 0; i++)
            {
                // Repeat while the current symbol still fits into num.
                while (values[i] <= num)
                {
                    num -= values[i];
                    string[] symbols = ["M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I"];
                    sb.Append(symbols[i]);
                }
            }
            return sb.ToString();
        }
        #endregion

        #region 隐私保护
        public static string ProctectMobile(string mobile)
        {
            if (string.IsNullOrEmpty(mobile)) return string.Empty;
            if (mobile.Length == 11)
            {
                return $"{mobile[0..3]}****{mobile[^4..]}";
            }

            return "****";
        }

        public static string ProctectEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            return "****@***.***";
        }
        #endregion

        #region 清除HTML标记（旧版创新网）
        public static string DropHTML(string htmlString)
        {
            if (string.IsNullOrEmpty(htmlString)) return "";

            //删除脚本  
            htmlString = Regex.Replace(htmlString, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML  
            htmlString = Regex.Replace(htmlString, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"-->", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"<!--.*", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);

            htmlString = Regex.Replace(htmlString, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            //Htmlstring.Replace("<", "");
            //Htmlstring.Replace(">", "");
            //Htmlstring.Replace("\r\n", "");
            htmlString = HttpUtility.HtmlEncode(htmlString).Trim();
            return htmlString;
        }

        public static string EscapeHtml(string htmlString)
        {
            try
            {
                var nodes = htmlChecker.ParseNode(htmlString);
                return nodes.GetInnerText();
            }
            catch
            {
                return htmlString;
            }
        }
        #endregion

        #region 生成图片占位符
        public static SKBitmap CreateImagePlaceHolder(int width, int height, string text)
        {
            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                var textPaint = new SKPaint()
                {
                    Color = new SKColor(150, 150, 150), //颜色
                    StrokeWidth = 1, //画笔宽度
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal), //字体
                    TextSize = 32,  //字体大小
                    Style = SKPaintStyle.StrokeAndFill,
                    IsAntialias = true,
                };

                var backgroundPaint = new SKPaint()
                {
                    Color = new SKColor(202, 202, 202),
                    StrokeWidth = 1,
                    Style = SKPaintStyle.StrokeAndFill,
                };

                canvas.DrawRect(0, 0, width, height, backgroundPaint);

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = $"{width} x {height}";
                }

                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float xText = width / 2 - textBounds.MidX;
                float yText = height / 2 - textBounds.MidY;
                canvas.DrawText(text, xText, yText, textPaint);
            }

            return bitmap;
        }

        public static byte[] CreateImagePlaceHolderBytes(int width, int height, string text)
        {
            SKBitmap bitmap = CreateImagePlaceHolder(width, height, text);
            MemoryStream ms = new MemoryStream();
            bitmap.Encode(ms, SKEncodedImageFormat.Jpeg, 100);
            return ms.ToArray();
        }
        #endregion

        /// <summary>
        /// 保证参数不为NULL
        /// </summary>
        /// <param name="arg"></param>
        /// <exception cref="ArgumentNullException"></exception>
        [return: NotNull]
        public static T EnsureNotNull<T>(T? arg, string message = "")
        {
            message = string.IsNullOrWhiteSpace(message) ? nameof(arg) : message;
            if (arg == null) throw new ArgumentNullException(message);
            return arg;
        }
    }
}
