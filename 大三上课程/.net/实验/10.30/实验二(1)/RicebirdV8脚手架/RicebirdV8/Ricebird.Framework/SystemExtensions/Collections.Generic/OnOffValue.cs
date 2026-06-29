namespace System.Collections.Generic
{
    /// <summary>
    /// 这是一个类似布尔值的量，可以与布尔值互相隐式转换
    /// <para>
    /// 与字符串隐式转换时，相当于判定 !string.IsNullOrWhiteSpace
    /// </para>
    /// <para>
    /// 与数值隐式转换时，相当于判定 数值 != 0
    /// </para>
    /// <para>
    /// 与数组隐式转换时，相当于判定 Array.Length != 0
    /// </para>
    /// </summary>
    public struct OnOffValue
    {
        internal bool Value { get; set; }

        public static implicit operator bool(OnOffValue? v)
        {
            return v?.Value ?? false;
        }

        public static implicit operator OnOffValue(string? str)
        {
            OnOffValue v = new()
            {
                Value = !string.IsNullOrWhiteSpace(str)
            };
            return v;
        }

        public static implicit operator OnOffValue(Guid? id)
        {
            OnOffValue v = new()
            {
                Value = id.HasValue && id.Value != Guid.Empty
            };
            return v;
        }

        public static implicit operator OnOffValue(DateTime? dt)
        {
            bool value = false;
            if (dt.HasValue)
            {
                value = (dt.Value != ConstKeys.MinDate && dt.Value != ConstKeys.MaxDate);
            }

            OnOffValue v = new()
            {
                Value = value
            };

            return v;
        }

        public static implicit operator OnOffValue(bool? value)
        {
            OnOffValue v = new()
            {
                Value = value ?? false
            };
            return v;
        }

        public static implicit operator OnOffValue(Array? list)
        {
            OnOffValue v = new()
            {
                Value = list?.Length != 0
            };
            return v;
        }

        public static implicit operator OnOffValue(string[] list)
        {
            OnOffValue v = new()
            {
                Value = list.All(e => !string.IsNullOrWhiteSpace(e))
            };
            return v;
        }

        #region 与数值隐式转换
        public static implicit operator OnOffValue(float? value)
        {
            OnOffValue v = new()
            {
                Value = (value != 0)
            };
            return v;
        }

        public static implicit operator OnOffValue(double? value)
        {
            OnOffValue v = new()
            {
                Value = (value != 0)
            };
            return v;
        }

        public static implicit operator OnOffValue(decimal? value)
        {
            OnOffValue v = new()
            {
                Value = (value != 0)
            };
            return v;
        }

        public static implicit operator OnOffValue(int? value)
        {
            OnOffValue v = new()
            {
                Value = (value != 0)
            };
            return v;
        }

        public static implicit operator OnOffValue(long? value)
        {
            OnOffValue v = new()
            {
                Value = (value != 0)
            };
            return v;
        }
        #endregion
    }
}
