using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class ValueLimit<T> : AbstactValidateRule<T>
        where T : struct, IComparable<T>
    {
        public override bool Multiple => false;
        public override string RuleName => "数值上下限限制";
        public const string DEFAULTMESSAGE = "{0}{1}限为{2}";

        public static readonly (int boundType, string boundStr) MAXBOUND = (1, "上");
        public static readonly (int boundType, string boundStr) MINBOUND = (-1, "下");

        /// <summary>
        /// 边界
        /// </summary>
        protected T Bound
        {
            get; set;
        }

        protected (int boundType, string boundStr) Type
        {
            get; set;
        }

        public ValueLimit()
        {
            Message = DEFAULTMESSAGE;
            Type = MINBOUND;
            Bound = default;
        }

        public ValueLimit(T bound, (int boundType, string boundStr) boundType, string message)
        {
            Bound = bound;
            Type = boundType;
            Message = message;
        }

        public override void Validate(IClient? client, ValidateResult result, T validateObj, string propertyName, string? fieldName, object? value)
        {
            if (value is IComparable<T> src)
            {
                if (src.CompareTo(Bound) == Type.boundType)
                {
                    result.SetFailure(propertyName, String.Format(Message, fieldName, Type.boundType, value));
                }
            }
        }

        public override object ToJsonObject(string? fieldName)
        {
            return new
            {
                valueLimit = true,
                message = string.Format(Message, fieldName)
            };
        }
    }
}
