using System.Globalization;
using System.Windows.Data;

namespace HomeworkGrader
{
    /// <summary>
    /// 布尔值到状态文本转换器
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGraded)
            {
                return isGraded ? "已批改" : "待批改";
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到迟到状态转换器
    /// </summary>
    public class BoolToLateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLate)
            {
                return isLate ? "⚠️ 迟到提交" : "✅ 按时提交";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 已批改数量转换器
    /// </summary>
    public class GradedCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int totalSubmissions)
            {
                // 这里简化处理，实际应该计算已批改的数量
                return totalSubmissions; // 临时返回总数，实际应该返回已批改数
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 百分比转换器
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int gradedCount)
            {
                // 这里需要获取总数来计算百分比，简化处理
                return $"{gradedCount}%"; // 临时返回，实际应该计算真实百分比
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

