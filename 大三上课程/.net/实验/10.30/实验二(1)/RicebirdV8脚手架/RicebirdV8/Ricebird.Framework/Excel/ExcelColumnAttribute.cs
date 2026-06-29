namespace Ricebird.Framework.Excel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>
        /// Excel数据列
        /// </summary>
        /// <param name="columnName">列名</param>
        public ExcelColumnAttribute(string columnName)
        {
            ColumnName = columnName;
            DisplayOrder = 0;
            ToStringMethod = (obj) => obj?.ToString() ?? "";
        }

        /// <summary>
        /// Excel数据列
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <param name="order">排序号（升），默认0</param>
        public ExcelColumnAttribute(string columnName, int order)
        {
            ColumnName = columnName;
            DisplayOrder = order;
            ToStringMethod = (obj) => obj?.ToString() ?? "";
        }

        /// <summary>
        /// Excel数据列
        /// </summary>
        /// <param name="columnName">列名</param>
        ///  /// <param name="order">排序号（升），默认0</param>
        /// <param name="toString">显示为字符串的形式</param>
        public ExcelColumnAttribute(string columnName, int order, Func<object, string> toString)
        {
            if (toString == null)
            {
                toString = (obj) => obj.ToString() ?? string.Empty;
            }

            ColumnName = columnName;
            DisplayOrder = order;
            ToStringMethod = toString;
        }

        public string ColumnName
        {
            get; set;
        } = string.Empty;

        public Func<object, string>? ToStringMethod
        {
            get; set;
        }

        public int DisplayOrder
        {
            get; set;
        } = 0;

        internal PropertyInfo? Property
        {
            get; set;
        }
    }
}
